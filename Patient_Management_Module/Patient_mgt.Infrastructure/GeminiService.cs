using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Patient_mgt.Infrastructure
{
    public interface IGeminiService
    {
        Task<List<LabTestResult>> ExtractLabTestsAsync(string text);
        Task<string> GenerateClinicalSummaryAsync(List<LabTestResult> labResults);

        Task<string> GenerateResponse(string prompt);
        
    }

    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
        private readonly string[] _models = { "gemini-2.5-flash", "gemini-2.0-flash", "gemini-flash-latest", "gemini-2.5-flash-lite", "gemini-2.0-flash-001" };

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentException("Gemini API key not configured");
        }

        public async Task<List<LabTestResult>> ExtractLabTestsAsync(string text)
        {
            var prompt = $@"Extract structured lab test data from this text. Return ONLY a JSON array of objects with this exact format:
[{{
  ""testName"": ""Test Name"",
  ""value"": ""numeric value"",
  ""unit"": ""unit"",
  ""referenceRange"": ""normal range"",
  ""isAbnormal"": true/false
}}]

Text: {text}";

            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Try each model until one works
            foreach (var model in _models)
            {
                try
                {
                    Console.WriteLine($"Trying model: {model}");
                    var response = await _httpClient.PostAsync($"{BaseUrl}/{model}:generateContent?key={_apiKey}", content);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Console.WriteLine($"Model {model} rate limited, trying next model...");
                        continue;
                    }
                    
                    response.EnsureSuccessStatusCode();

                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<GeminiResponse>(responseJson);
                    
                    var jsonText = result?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text ?? "[]";
                    
                    // Clean the JSON response
                    jsonText = jsonText.Trim().Replace("```json", "").Replace("```", "").Trim();
                    
                    try
                    {
                        var labResults = JsonSerializer.Deserialize<List<LabTestResult>>(jsonText) ?? new List<LabTestResult>();
                        Console.WriteLine($"Successfully used model: {model}");
                        return labResults;
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to parse JSON from model: {model}, trying next model...");
                        continue;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Model {model} failed with error: {ex.Message}, trying next model...");
                    continue;
                }
            }
            
            Console.WriteLine("All models failed or rate limited");
            
            // Fallback: Try basic text parsing
            Console.WriteLine("Attempting basic fallback parsing...");
            return ParseLabResultsBasic(text);
        }

        public async Task<string> GenerateClinicalSummaryAsync(List<LabTestResult> labResults)
        {
            var labData = JsonSerializer.Serialize(labResults, new JsonSerializerOptions { WriteIndented = true });
            
            var prompt = $@"Analyze these lab results and return a clinical summary using EXACTLY this format. Do not deviate.

Lab Report Summary

🔴 ABNORMAL RESULTS:
• [TestName]: [Value] [Unit] ([High/Low]) [Ref: RefRange]

📊 KEY FINDINGS:
[2-3 sentences about the most important findings]

⚠️ CLINICAL SIGNIFICANCE:
[1-2 sentences about what these abnormalities might indicate]

🔍 RECOMMENDED FOLLOW-UP:
• [action 1]
• [action 2]
• [action 3]

STRICT RULES:
- Use exactly the section headers above with their emoji, nothing else
- Abnormal results line format must be: • TestName: Value Unit (High/Low) [Ref: range]
- Use • for every bullet point
- One blank line between sections, no more
- No markdown bold, no asterisks, no extra headers
- Under 200 words total

Lab Results:
{labData}";

            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Try each model until one works
            foreach (var model in _models)
            {
                try
                {
                    Console.WriteLine($"Trying model for clinical summary: {model}");
                    var response = await _httpClient.PostAsync($"{BaseUrl}/{model}:generateContent?key={_apiKey}", content);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Console.WriteLine($"Model {model} rate limited for clinical summary, trying next model...");
                        continue;
                    }
                    
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine($"Model {model} responded successfully for clinical summary");

                    var responseJson = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response JSON length: {responseJson?.Length ?? 0}");
                    
                    var result = JsonSerializer.Deserialize<GeminiResponse>(responseJson);
                    
                    var summary = result?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text ?? "";
                    
                    if (!string.IsNullOrEmpty(summary))
                    {
                        // Clean up the summary for better UI display
                        summary = CleanClinicalSummary(summary);
                        Console.WriteLine($"Successfully generated clinical summary using model: {model}, length: {summary.Length}");
                        Console.WriteLine($"Summary preview: {summary.Substring(0, Math.Min(100, summary.Length))}...");
                        return summary;
                    }
                    else
                    {
                        Console.WriteLine($"Model {model} returned empty summary, trying next model...");
                        continue;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Model {model} failed for clinical summary with error: {ex.Message}, trying next model...");
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error with model {model}: {ex.Message}, trying next model...");
                    continue;
                }
            }
            
            Console.WriteLine("All models failed or rate limited for clinical summary");
            return "Clinical summary generation failed - all models are currently rate limited. Please try again later.";
        }

        private List<LabTestResult> ParseLabResultsBasic(string text)
        {
            var results = new List<LabTestResult>();
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            Console.WriteLine($"Basic parsing: Processing {lines.Length} lines of text");
            Console.WriteLine($"Text preview: {text.Substring(0, Math.Min(300, text.Length))}");
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;
                
                Console.WriteLine($"Processing line: {trimmedLine}");
                
                // Look for patterns like "TestName: Value Unit (Range)" or "TestName Value Unit Range"
                var parts = trimmedLine.Split(new[] { ':', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var testName = parts[0].Trim();
                    var remainingText = string.Join(" ", parts.Skip(1)).Trim();
                    
                    // Skip if it looks like a header or non-test line
                    if (testName.Length > 50 || 
                        testName.ToLower().Contains("report") || 
                        testName.ToLower().Contains("date") ||
                        testName.ToLower().Contains("patient") ||
                        testName.ToLower().Contains("doctor") ||
                        testName.ToLower().Contains("lab") ||
                        testName.Length < 3)
                        continue;
                        
                    // Extract numeric value - look for any number (including decimals)
                    var valueMatch = System.Text.RegularExpressions.Regex.Match(remainingText, @"([0-9]+\.?[0-9]*)");
                    if (valueMatch.Success)
                    {
                        var value = valueMatch.Groups[1].Value;
                        var unit = ExtractUnit(remainingText);
                        var range = ExtractRange(remainingText);
                        var isAbnormal = IsAbnormalValue(remainingText, value, range);
                        
                        var result = new LabTestResult
                        {
                            testName = testName,
                            value = value,
                            unit = unit,
                            referenceRange = range,
                            isAbnormal = isAbnormal
                        };
                        
                        results.Add(result);
                        Console.WriteLine($"Found lab result: {testName} = {value} {unit} (Range: {range}, Abnormal: {isAbnormal})");
                    }
                }
                
                // Also try to parse common lab report formats like "Glucose 95 mg/dL (70-100)"
                var labMatch = System.Text.RegularExpressions.Regex.Match(trimmedLine, 
                    @"^([A-Za-z][A-Za-z\s]+?)\s+([0-9]+\.?[0-9]*)\s*([A-Za-z/µ%]*)?\s*(?:\(([^)]+)\))?");
                    
                if (labMatch.Success && !results.Any(r => r.testName.Equals(labMatch.Groups[1].Value.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    var testName = labMatch.Groups[1].Value.Trim();
                    var value = labMatch.Groups[2].Value;
                    var unit = labMatch.Groups[3].Value;
                    var range = labMatch.Groups[4].Value;
                    
                    if (testName.Length >= 3 && testName.Length <= 30)
                    {
                        var isAbnormal = IsAbnormalValue(trimmedLine, value, range);
                        
                        var result = new LabTestResult
                        {
                            testName = testName,
                            value = value,
                            unit = unit,
                            referenceRange = range,
                            isAbnormal = isAbnormal
                        };
                        
                        results.Add(result);
                        Console.WriteLine($"Found lab result (regex): {testName} = {value} {unit} (Range: {range}, Abnormal: {isAbnormal})");
                    }
                }
            }
            
            Console.WriteLine($"Basic parsing found {results.Count} potential lab results");
            return results;
        }
        
        private string ExtractUnit(string text)
        {
            // Look for common lab units
            var unitMatch = System.Text.RegularExpressions.Regex.Match(text, @"\b(mg/dL|g/dL|mmol/L|µg/dL|ng/mL|mL|L|%|cells/µL|/µL|U/L|IU/L|pg/mL|µIU/mL)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (unitMatch.Success) return unitMatch.Groups[1].Value;
            
            // Fallback to any alphabetic characters after numbers
            var fallbackMatch = System.Text.RegularExpressions.Regex.Match(text, @"[0-9]+\.?[0-9]*\s*([a-zA-Z/µ%]+)");
            return fallbackMatch.Success ? fallbackMatch.Groups[1].Value : "";
        }
        
        private string ExtractRange(string text)
        {
            // Look for ranges in parentheses like (70-100) or (3.5-5.0)
            var rangeMatch = System.Text.RegularExpressions.Regex.Match(text, @"\(([0-9]+\.?[0-9]*\s*-\s*[0-9]+\.?[0-9]*)\)");
            if (rangeMatch.Success) return rangeMatch.Groups[1].Value;
            
            // Look for ranges without parentheses like "Normal: 70-100"
            var normalMatch = System.Text.RegularExpressions.Regex.Match(text, @"(?:Normal|Range|Ref):?\s*([0-9]+\.?[0-9]*\s*-\s*[0-9]+\.?[0-9]*)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return normalMatch.Success ? normalMatch.Groups[1].Value : "";
        }
        
        private bool IsAbnormalValue(string text, string value, string range)
        {
            // Check for explicit abnormal indicators
            var lowerText = text.ToLower();
            if (lowerText.Contains("high") || lowerText.Contains("low") || 
                lowerText.Contains("abnormal") || lowerText.Contains("elevated") ||
                lowerText.Contains("decreased") || lowerText.Contains("*"))
                return true;
            
            // Try to determine if value is outside range
            if (!string.IsNullOrEmpty(range) && double.TryParse(value, out double numValue))
            {
                var rangeParts = range.Split('-');
                if (rangeParts.Length == 2 && 
                    double.TryParse(rangeParts[0].Trim(), out double minRange) &&
                    double.TryParse(rangeParts[1].Trim(), out double maxRange))
                {
                    return numValue < minRange || numValue > maxRange;
                }
            }
            
            return false;
        }
        
        private string CleanClinicalSummary(string summary)
        {
            if (string.IsNullOrEmpty(summary)) return summary;

            // Remove markdown bold/italic markers
            summary = System.Text.RegularExpressions.Regex.Replace(summary, @"\*{1,2}([^*]+)\*{1,2}", "$1");

            // Remove markdown headers
            summary = System.Text.RegularExpressions.Regex.Replace(summary, @"#{1,6}\s*", "");

            // Normalize bullet points to •
            summary = System.Text.RegularExpressions.Regex.Replace(summary, @"^\s*[\*\-]\s+", "• ", System.Text.RegularExpressions.RegexOptions.Multiline);

            // Collapse 3+ consecutive newlines to exactly two
            summary = System.Text.RegularExpressions.Regex.Replace(summary, @"\n{3,}", "\n\n");

            return summary.Trim();
        }

        //llm response for RAG response
        // 🤖 FINAL LLM RESPONSE (Hybrid RAG)
        public async Task<string> GenerateResponse(string prompt)
        {
            var request = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
            };

            var json = JsonSerializer.Serialize(request);

            foreach (var model in _models)
            {
                try
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(
                        $"{BaseUrl}/{model}:generateContent?key={_apiKey}",
                        content
                    );

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        continue;

                    response.EnsureSuccessStatusCode();

                    var responseJson = await response.Content.ReadAsStringAsync();

                    var result = JsonSerializer.Deserialize<GeminiResponse>(responseJson);

                    var answer = result?
                        .candidates?
                        .FirstOrDefault()?
                        .content?
                        .parts?
                        .FirstOrDefault()?
                        .text;

                    if (!string.IsNullOrWhiteSpace(answer))
                        return answer.Trim();
                }
                catch
                {
                    continue;
                }
            }

            return "I'm unable to answer at the moment. Please try again.";
        }
    }

    public class LabTestResult
    {
        public string testName { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
        public string? unit { get; set; }
        public string? referenceRange { get; set; }
        public bool isAbnormal { get; set; }
    }

    public class GeminiResponse
    {
        public GeminiCandidate[]? candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public GeminiContent? content { get; set; }
    }

    public class GeminiContent
    {
        public GeminiPart[]? parts { get; set; }
    }

    public class GeminiPart
    {
        public string? text { get; set; }
    }




}