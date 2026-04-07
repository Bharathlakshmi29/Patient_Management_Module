using AutoMapper;
using Microsoft.AspNetCore.Http;
using Patient_mgt.Application;
using Patient_mgt.Domain;
using Patient_mgt.DTOs;

namespace Patient_mgt.Infrastructure
{
    public class MedicalReportService : IMedicalReportService
    {
        private readonly IMedicalReportRepository _repo;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IOcrService _ocrService;
        private readonly IGeminiService _geminiService;

        private static readonly HashSet<ReportType> AnalysableTypes = new()
        {
            ReportType.LAB_REPORT,
            ReportType.BLOOD_TEST,
            ReportType.URINE_TEST
        };

        public MedicalReportService(
            IMedicalReportRepository repo,
            IMapper mapper,
            ICloudinaryService cloudinaryService,
            IOcrService ocrService,
            IGeminiService geminiService)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _ocrService = ocrService;
            _geminiService = geminiService;
        }

        public async Task<IEnumerable<MedicalReportDTO>> GetReportsByPatientId(int patientId)
        {
            var reports = await _repo.GetReportsByPatientId(patientId);
            return _mapper.Map<IEnumerable<MedicalReportDTO>>(reports);
        }

        public async Task<MedicalReportDTO?> GetReportById(int reportId)
        {
            var report = await _repo.GetReportById(reportId);
            if (report == null) return null;
            return _mapper.Map<MedicalReportDTO>(report);
        }

        public async Task<MedicalReportDTO> CreateReport(CreateMedicalReportDTO dto)
        {
            var report = _mapper.Map<MedicalReport>(dto);
            report.ReportType = (ReportType)dto.ReportType;

            byte[]? fileBytes = null;

            if (dto.File != null)
            {
                using var ms = new MemoryStream();
                await dto.File.CopyToAsync(ms);
                fileBytes = ms.ToArray();
                
                // Upload to Cloudinary
                report.FileUrl = await _cloudinaryService.UploadDocumentAsync(
                    fileBytes, 
                    dto.File.FileName, 
                    "medical-reports"
                );
                
                // Extract public ID for future operations
                var uri = new Uri(report.FileUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                var publicIdWithExtension = string.Join("/", pathSegments.Skip(4)); // Skip /v1234567890/
                report.CloudinaryPublicId = Path.GetFileNameWithoutExtension(publicIdWithExtension);
                
                // Set file properties
                report.FileName = dto.File.FileName;
                report.FileType = dto.File.ContentType;
                report.FileSize = dto.File.Length;
            }

            var created = await _repo.CreateReport(report);

            // Auto-analyse if it's a lab/blood/urine report — run inline since bytes already in memory
            if (AnalysableTypes.Contains(created.ReportType) && fileBytes != null)
                await AnalyseReportAsync(created.ReportId, fileBytes);

            return _mapper.Map<MedicalReportDTO>(created);
        }

        public async Task UpdateReport(int reportId, UpdateMedicalReportDTO dto)
        {
            var existingReport = await _repo.GetReportById(reportId);
            if (existingReport == null)
                throw new Exception($"Medical report with ID {reportId} not found.");

            // Update basic properties
            existingReport.ReportType = (ReportType)dto.ReportType;
            existingReport.ReportName = dto.ReportName;
            existingReport.Description = dto.Description;
            existingReport.UpdatedAt = DateTime.UtcNow;

            if (dto.File != null)
            {
                if (!string.IsNullOrEmpty(existingReport.CloudinaryPublicId))
                    await _cloudinaryService.DeleteFileAsync(existingReport.CloudinaryPublicId);

                using var ms = new MemoryStream();
                await dto.File.CopyToAsync(ms);
                var updatedFileBytes = ms.ToArray();

                existingReport.FileUrl = await _cloudinaryService.UploadDocumentAsync(
                    updatedFileBytes,
                    dto.File.FileName,
                    "medical-reports"
                );

                var uri = new Uri(existingReport.FileUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                var publicIdWithExtension = string.Join("/", pathSegments.Skip(4));
                existingReport.CloudinaryPublicId = Path.GetFileNameWithoutExtension(publicIdWithExtension);

                existingReport.FileName = dto.File.FileName;
                existingReport.FileType = dto.File.ContentType;
                existingReport.FileSize = dto.File.Length;

                await _repo.UpdateReport(reportId, existingReport);

                if (AnalysableTypes.Contains(existingReport.ReportType))
                    await AnalyseReportAsync(reportId, updatedFileBytes);

                return;
            }

            await _repo.UpdateReport(reportId, existingReport);
        }

        public async Task<bool> DeleteReport(int reportId)
        {
            try
            {
                var report = await _repo.GetReportById(reportId);
                if (report != null && !string.IsNullOrEmpty(report.CloudinaryPublicId))
                {
                    // Delete from Cloudinary
                    await _cloudinaryService.DeleteFileAsync(report.CloudinaryPublicId);
                }
                
                await _repo.DeleteReport(reportId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<byte[]> DownloadReport(int reportId)
        {
            var report = await _repo.GetReportById(reportId);
            if (report == null)
                throw new Exception($"Medical report with ID {reportId} not found.");

            if (!string.IsNullOrEmpty(report.FileUrl))
                return await _cloudinaryService.DownloadFileAsync(report.FileUrl);

            throw new Exception("File URL not available.");
        }

        private async Task AnalyseReportAsync(int reportId, byte[] fileBytes)
        {
            try
            {
                // Step 1: Extract text — route JSON/FHIR directly, everything else via OCR
                string extractedText;
                if (IsJsonFile(fileBytes))
                {
                    extractedText = System.Text.Encoding.UTF8.GetString(fileBytes);
                    Console.WriteLine($"Report {reportId}: detected JSON/FHIR file, skipping OCR.");
                }
                else
                {
                    extractedText = await _ocrService.ExtractTextFromImageAsync(fileBytes);
                    Console.WriteLine($"Report {reportId}: processed via OCR.");
                }

                if (string.IsNullOrWhiteSpace(extractedText))
                    return;

                // Step 2: Gemini — extract structured lab results
                var labResults = await _geminiService.ExtractLabTestsAsync(extractedText);
                if (labResults == null || labResults.Count == 0)
                    return;

                // Step 3: Gemini — generate clinical summary
                var summary = await _geminiService.GenerateClinicalSummaryAsync(labResults);
                if (string.IsNullOrWhiteSpace(summary))
                    return;

                // Step 4: Save analysis result to DB as structured JSON (same format as LabReportController)
                var abnormalResults = labResults.Where(lt => lt.isAbnormal).Select(lt => new
                {
                    testName = lt.testName,
                    value = lt.value,
                    unit = lt.unit,
                    referenceRange = lt.referenceRange,
                    status = "ABNORMAL"
                }).ToList();

                var normalResults = labResults.Where(lt => !lt.isAbnormal).Select(lt => new
                {
                    testName = lt.testName,
                    value = lt.value,
                    unit = lt.unit,
                    referenceRange = lt.referenceRange,
                    status = "NORMAL"
                }).ToList();

                var analysisResult = new
                {
                    Summary = new
                    {
                        totalTests = labResults.Count,
                        abnormalCount = abnormalResults.Count,
                        normalCount = normalResults.Count
                    },
                    ClinicalSummary = summary,
                    AbnormalResults = (object)abnormalResults,
                    NormalResults = (object)normalResults
                };

                var analysisJson = System.Text.Json.JsonSerializer.Serialize(analysisResult);
                await _repo.UpdateAnalysisResult(reportId, analysisJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auto-analysis failed for report {reportId}: {ex.Message}");
            }
        }
        private static bool IsJsonFile(byte[] fileBytes)
        {
            int i = 0;
            while (i < fileBytes.Length && (fileBytes[i] == 0x20 || fileBytes[i] == 0x09 ||
                                             fileBytes[i] == 0x0A || fileBytes[i] == 0x0D))
                i++;
            return i < fileBytes.Length && (fileBytes[i] == (byte)'{' || fileBytes[i] == (byte)'[');
        }
    }
}