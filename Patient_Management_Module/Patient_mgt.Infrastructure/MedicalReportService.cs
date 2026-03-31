using AutoMapper;
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

        public MedicalReportService(IMedicalReportRepository repo, IMapper mapper, ICloudinaryService cloudinaryService)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
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
            
            // Convert integer ReportType to enum
            report.ReportType = (ReportType)dto.ReportType;
            
            if (dto.File != null)
            {
                using var ms = new MemoryStream();
                await dto.File.CopyToAsync(ms);
                var fileBytes = ms.ToArray();
                
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

            // Handle file update if provided
            if (dto.File != null)
            {
                // Delete old file from Cloudinary if exists
                if (!string.IsNullOrEmpty(existingReport.CloudinaryPublicId))
                {
                    await _cloudinaryService.DeleteFileAsync(existingReport.CloudinaryPublicId);
                }

                using var ms = new MemoryStream();
                await dto.File.CopyToAsync(ms);
                var fileBytes = ms.ToArray();
                
                // Upload new file to Cloudinary
                existingReport.FileUrl = await _cloudinaryService.UploadDocumentAsync(
                    fileBytes, 
                    dto.File.FileName, 
                    "medical-reports"
                );
                
                // Extract public ID for future operations
                var uri = new Uri(existingReport.FileUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                var publicIdWithExtension = string.Join("/", pathSegments.Skip(4));
                existingReport.CloudinaryPublicId = Path.GetFileNameWithoutExtension(publicIdWithExtension);
                
                // Update file properties
                existingReport.FileName = dto.File.FileName;
                existingReport.FileType = dto.File.ContentType;
                existingReport.FileSize = dto.File.Length;
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

            // Download from Cloudinary
            if (!string.IsNullOrEmpty(report.FileUrl))
            {
                return await _cloudinaryService.DownloadFileAsync(report.FileUrl);
            }

            throw new Exception("File URL not available.");
        }
    }
}