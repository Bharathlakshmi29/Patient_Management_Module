using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patient_mgt.Infrastructure;
using Patient_mgt.Application;
using Patient_mgt.Domain;
using System.Text.Json;

namespace Patient_Management_Module.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Temporarily disabled for testing
    public class LabReportController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly IOcrService _ocrService;
        private readonly IMedicalReportRepository _medicalReportRepository;
        private readonly IConfiguration _configuration;

        public LabReportController(
            IGeminiService geminiService,
            IOcrService ocrService,
            IMedicalReportRepository medicalReportRepository,
            IConfiguration configuration)
        {
            _geminiService = geminiService;
            _ocrService = ocrService;
            _medicalReportRepository = medicalReportRepository;
            _configuration = configuration;
        }

        [HttpPost("analyze/{reportId}")]
        public async Task<IActionResult> AnalyzeLabReport(int reportId)
        {
            try
            {
                Console.WriteLine($"Starting analysis for report ID: {reportId}");
                
                // Get the medical report
                var report = await _medicalReportRepository.GetReportById(reportId);
                if (report == null)
                {
                    Console.WriteLine($"Report {reportId} not found in database");
                    return NotFound($"Medical report with ID {reportId} not found.");
                }

                // Check if analysis already exists
                if (!string.IsNullOrEmpty(report.AnalysisResult))
                {
                    Console.WriteLine($"Analysis already exists for report {reportId}, returning cached result");
                    
                    try
                    {
                        var existingAnalysis = JsonSerializer.Deserialize<AnalysisResultDto>(report.AnalysisResult);
                        return Ok(new
                        {
                            message = "Analysis retrieved from cache",
                            cached = true,
                            analyzedAt = report.AnalyzedAt,
                            summary = existingAnalysis.Summary,
                            clinicalSummary = existingAnalysis.ClinicalSummary,
                            abnormalResults = existingAnalysis.AbnormalResults,
                            normalResults = existingAnalysis.NormalResults
                        });
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Failed to deserialize existing analysis: {ex.Message}. Proceeding with new analysis.");
                        // Continue with new analysis if deserialization fails
                    }
                }

                Console.WriteLine($"No existing analysis found. Processing report: {report.ReportName}, FileUrl: {report.FileUrl}");
                
                if (string.IsNullOrEmpty(report.FileUrl))
                {
                    Console.WriteLine($"Report {reportId} has no file URL");
                    return BadRequest("Report file URL is not available.");
                }

                // Download the file
                byte[] fileBytes;
                try
                {
                    Console.WriteLine($"Downloading file from: {report.FileUrl}");
                    using var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(30);
                    fileBytes = await httpClient.GetByteArrayAsync(report.FileUrl);
                    Console.WriteLine($"File downloaded successfully, size: {fileBytes.Length} bytes");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"File download failed: {ex.Message}");
                    return BadRequest($"Failed to download file: {ex.Message}");
                }

                // Step 1: OCR - Extract text from image/PDF
                string extractedText;
                try
                {
                    Console.WriteLine("Starting OCR extraction...");
                    extractedText = await _ocrService.ExtractTextFromImageAsync(fileBytes);
                    Console.WriteLine($"OCR completed, extracted text length: {extractedText?.Length ?? 0}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OCR extraction failed: {ex.Message}");
                    return StatusCode(500, $"OCR extraction failed: {ex.Message}");
                }

                if (string.IsNullOrEmpty(extractedText))
                {
                    Console.WriteLine("No text extracted from the report");
                    return BadRequest("Could not extract text from the report.");
                }

                // Step 2: LLM 1 - Extract structured lab tests
                List<LabTestResult> labTests;
                try
                {
                    Console.WriteLine("Starting lab test extraction...");
                    labTests = await _geminiService.ExtractLabTestsAsync(extractedText);
                    Console.WriteLine($"Lab test extraction completed, found {labTests?.Count ?? 0} tests");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lab test extraction failed: {ex.Message}");
                    return StatusCode(500, $"Lab test extraction failed: {ex.Message}");
                }

                if (!labTests.Any())
                {
                    Console.WriteLine("No lab tests found in the extracted text");
                    return BadRequest("No lab tests found in the report.");
                }

                // Step 3: LLM 2 - Generate clinical summary
                string clinicalSummary;
                try
                {
                    Console.WriteLine("Starting clinical summary generation...");
                    clinicalSummary = await _geminiService.GenerateClinicalSummaryAsync(labTests);
                    Console.WriteLine($"Clinical summary generated, length: {clinicalSummary?.Length ?? 0}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Clinical summary generation failed: {ex.Message}");
                    return StatusCode(500, $"Clinical summary generation failed: {ex.Message}");
                }

                // Prepare analysis result
                var abnormalResults = labTests.Where(lt => lt.isAbnormal).Select(lt => new
                {
                    testName = lt.testName,
                    value = lt.value,
                    unit = lt.unit,
                    referenceRange = lt.referenceRange,
                    status = "ABNORMAL"
                }).ToList();

                var normalResults = labTests.Where(lt => !lt.isAbnormal).Select(lt => new
                {
                    testName = lt.testName,
                    value = lt.value,
                    unit = lt.unit,
                    referenceRange = lt.referenceRange,
                    status = "NORMAL"
                }).ToList();

                var analysisResult = new AnalysisResultDto
                {
                    Summary = new
                    {
                        totalTests = labTests.Count,
                        abnormalCount = abnormalResults.Count,
                        normalCount = normalResults.Count
                    },
                    ClinicalSummary = clinicalSummary,
                    AbnormalResults = abnormalResults,
                    NormalResults = normalResults
                };

                // Store analysis result in database
                try
                {
                    var analysisJson = JsonSerializer.Serialize(analysisResult);
                    await _medicalReportRepository.UpdateAnalysisResult(reportId, analysisJson);
                    Console.WriteLine($"Analysis result stored successfully for report {reportId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to store analysis result: {ex.Message}");
                    // Continue and return result even if storage fails
                }

                Console.WriteLine($"Analysis completed successfully for report {reportId}");
                
                return Ok(new
                {
                    message = "Lab report analyzed successfully",
                    cached = false,
                    analyzedAt = DateTime.UtcNow,
                    summary = analysisResult.Summary,
                    clinicalSummary = analysisResult.ClinicalSummary,
                    abnormalResults = analysisResult.AbnormalResults,
                    normalResults = analysisResult.NormalResults
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error analyzing report {reportId}: {ex.Message}");
                Console.WriteLine($"Full stack trace: {ex.StackTrace}");
                return StatusCode(500, $"Error analyzing lab report: {ex.Message}");
            }
        }

        [HttpGet("results/{reportId}")]
        public async Task<IActionResult> GetLabResults(int reportId)
        {
            try
            {
                var report = await _medicalReportRepository.GetReportById(reportId);
                if (report == null)
                {
                    return NotFound($"Medical report with ID {reportId} not found.");
                }

                if (string.IsNullOrEmpty(report.AnalysisResult))
                {
                    return Ok(new { message = "No analysis results found for this report. Please analyze the report first." });
                }

                var analysisResult = JsonSerializer.Deserialize<AnalysisResultDto>(report.AnalysisResult);
                return Ok(new
                {
                    reportId = reportId,
                    analyzedAt = report.AnalyzedAt,
                    summary = analysisResult.Summary,
                    clinicalSummary = analysisResult.ClinicalSummary,
                    abnormalResults = analysisResult.AbnormalResults,
                    normalResults = analysisResult.NormalResults
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving lab results for report {reportId}: {ex.Message}");
                return StatusCode(500, $"Error retrieving lab results: {ex.Message}");
            }
        }

        [HttpDelete("analysis/{reportId}")]
        public async Task<IActionResult> ClearAnalysis(int reportId)
        {
            try
            {
                await _medicalReportRepository.UpdateAnalysisResult(reportId, null);
                return Ok(new { message = "Analysis cleared successfully. You can now re-analyze the report." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing analysis for report {reportId}: {ex.Message}");
                return StatusCode(500, $"Error clearing analysis: {ex.Message}");
            }
        }

        [HttpGet("test")]
        public IActionResult TestEndpoint()
        {
            try
            {
                return Ok(new { 
                    message = "LabReport controller is working",
                    geminiService = _geminiService != null ? "Available" : "Null",
                    ocrService = _ocrService != null ? "Available" : "Null",
                    medicalReportRepository = _medicalReportRepository != null ? "Available" : "Null"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Test failed: {ex.Message}");
            }
        }
    }

    // DTO for storing analysis results
    public class AnalysisResultDto
    {
        public object Summary { get; set; }
        public string ClinicalSummary { get; set; }
        public object AbnormalResults { get; set; }
        public object NormalResults { get; set; }
    }
}