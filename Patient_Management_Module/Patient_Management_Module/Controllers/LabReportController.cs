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

                // Step 1: Extract text — route to OCR (image/PDF), JSON parser, or FHIR parser
                string extractedText;
                try
                {
                    Console.WriteLine("Detecting file format and extracting text...");
                    extractedText = await ExtractTextFromFileAsync(fileBytes);
                    Console.WriteLine($"Text extraction completed, length: {extractedText?.Length ?? 0}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Text extraction failed: {ex.Message}");
                    return StatusCode(500, $"Text extraction failed: {ex.Message}");
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

        /// <summary>
        /// Detects whether the file is a JSON/FHIR document or a binary (image/PDF),
        /// and extracts human-readable lab text accordingly.
        /// </summary>
        private async Task<string> ExtractTextFromFileAsync(byte[] fileBytes)
        {
            // JSON detection: first non-whitespace byte is '{' or '['
            int startIdx = 0;
            while (startIdx < fileBytes.Length && (fileBytes[startIdx] == 0x20 || fileBytes[startIdx] == 0x09 ||
                                                    fileBytes[startIdx] == 0x0A || fileBytes[startIdx] == 0x0D))
                startIdx++;

            if (startIdx < fileBytes.Length && (fileBytes[startIdx] == (byte)'{' || fileBytes[startIdx] == (byte)'['))
            {
                var jsonText = System.Text.Encoding.UTF8.GetString(fileBytes);
                JsonDocument doc;
                try { doc = JsonDocument.Parse(jsonText); }
                catch (JsonException ex)
                {
                    Console.WriteLine($"File starts with '{{' but is not valid JSON: {ex.Message}. Falling back to OCR.");
                    return await _ocrService.ExtractTextFromImageAsync(fileBytes);
                }

                using (doc)
                {
                    var root = doc.RootElement;

                    // Check for FHIR resource type
                    if (root.TryGetProperty("resourceType", out var resourceTypeProp))
                    {
                        var resourceType = resourceTypeProp.GetString();
                        Console.WriteLine($"Detected FHIR resource: {resourceType}");

                        if (resourceType == "Bundle")
                            return ExtractTextFromFhirBundle(root);

                        if (resourceType == "DiagnosticReport")
                            return ExtractTextFromFhirDiagnosticReport(root);

                        if (resourceType == "Observation")
                            return ExtractTextFromFhirObservation(root);

                        // Unknown FHIR resource — pass JSON as-is to Gemini
                        Console.WriteLine($"Unknown FHIR resource type '{resourceType}', passing raw JSON.");
                        return jsonText;
                    }

                    // Plain JSON (not FHIR) — pass directly to Gemini
                    Console.WriteLine("Detected plain JSON lab data, passing directly to Gemini.");
                    return jsonText;
                }
            }

            // Binary file — use OCR (handles PDF, PNG, JPEG, etc.)
            Console.WriteLine("Detected binary file (image/PDF), routing to OCR.");
            return await _ocrService.ExtractTextFromImageAsync(fileBytes);
        }

        private string ExtractTextFromFhirBundle(JsonElement bundle)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("FHIR Bundle Lab Report");
            sb.AppendLine("======================");

            if (!bundle.TryGetProperty("entry", out var entries))
                return "FHIR Bundle contained no entries.";

            foreach (var entry in entries.EnumerateArray())
            {
                if (!entry.TryGetProperty("resource", out var resource)) continue;
                if (!resource.TryGetProperty("resourceType", out var rtProp)) continue;

                var rt = rtProp.GetString();
                if (rt == "Observation")
                    sb.AppendLine(ExtractTextFromFhirObservation(resource));
                else if (rt == "DiagnosticReport")
                    sb.AppendLine(ExtractTextFromFhirDiagnosticReport(resource));
            }

            var result = sb.ToString().Trim();
            Console.WriteLine($"Extracted FHIR Bundle text ({result.Length} chars).");
            return result;
        }

        private string ExtractTextFromFhirDiagnosticReport(JsonElement report)
        {
            var sb = new System.Text.StringBuilder();

            // Report title/name
            if (report.TryGetProperty("code", out var code) &&
                code.TryGetProperty("text", out var codeText))
                sb.AppendLine($"Report: {codeText.GetString()}");

            // Status
            if (report.TryGetProperty("status", out var status))
                sb.AppendLine($"Status: {status.GetString()}");

            // Conclusion / text narrative
            if (report.TryGetProperty("conclusion", out var conclusion))
                sb.AppendLine($"Conclusion: {conclusion.GetString()}");

            if (report.TryGetProperty("text", out var narrative) &&
                narrative.TryGetProperty("div", out var div))
                sb.AppendLine(div.GetString());

            return sb.ToString().Trim();
        }

        private string ExtractTextFromFhirObservation(JsonElement obs)
        {
            var sb = new System.Text.StringBuilder();

            // Test name
            string testName = "Unknown Test";
            if (obs.TryGetProperty("code", out var code))
            {
                if (code.TryGetProperty("text", out var codeText))
                    testName = codeText.GetString() ?? testName;
                else if (code.TryGetProperty("coding", out var coding) &&
                         coding.GetArrayLength() > 0 &&
                         coding[0].TryGetProperty("display", out var display))
                    testName = display.GetString() ?? testName;
            }

            // Value
            string value = "";
            string unit = "";
            if (obs.TryGetProperty("valueQuantity", out var vq))
            {
                if (vq.TryGetProperty("value", out var val)) value = val.ToString();
                if (vq.TryGetProperty("unit", out var u)) unit = u.GetString() ?? "";
            }
            else if (obs.TryGetProperty("valueString", out var vs))
            {
                value = vs.GetString() ?? "";
            }
            else if (obs.TryGetProperty("valueCodeableConcept", out var vcc) &&
                     vcc.TryGetProperty("text", out var vccText))
            {
                value = vccText.GetString() ?? "";
            }

            // Reference range
            string refRange = "";
            if (obs.TryGetProperty("referenceRange", out var ranges) && ranges.GetArrayLength() > 0)
            {
                var r = ranges[0];
                if (r.TryGetProperty("text", out var rText))
                    refRange = rText.GetString() ?? "";
                else
                {
                    string low = r.TryGetProperty("low", out var lo) && lo.TryGetProperty("value", out var lv) ? lv.ToString() : "";
                    string high = r.TryGetProperty("high", out var hi) && hi.TryGetProperty("value", out var hv) ? hv.ToString() : "";
                    if (!string.IsNullOrEmpty(low) || !string.IsNullOrEmpty(high))
                        refRange = $"{low}-{high}";
                }
            }

            // Interpretation (normal/abnormal)
            string interpretation = "";
            if (obs.TryGetProperty("interpretation", out var interps) && interps.GetArrayLength() > 0)
            {
                var interp = interps[0];
                if (interp.TryGetProperty("text", out var interpText))
                    interpretation = interpText.GetString() ?? "";
                else if (interp.TryGetProperty("coding", out var ic) && ic.GetArrayLength() > 0 &&
                         ic[0].TryGetProperty("code", out var ic0))
                    interpretation = ic0.GetString() ?? "";
            }

            sb.Append($"{testName}: {value} {unit}".Trim());
            if (!string.IsNullOrEmpty(refRange)) sb.Append($" (Ref: {refRange})");
            if (!string.IsNullOrEmpty(interpretation)) sb.Append($" [{interpretation}]");

            return sb.ToString();
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