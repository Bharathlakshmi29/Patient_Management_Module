using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Patient_mgt.Infrastructure
{
    public class OcrService : IOcrService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;

        public OcrService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OcrSettings:ApiKey"] ?? throw new ArgumentException("OCR API key not configured");
            _endpoint = configuration["OcrSettings:Endpoint"] ?? throw new ArgumentException("OCR endpoint not configured");
        }

        public async Task<string> ExtractTextFromImageAsync(byte[] imageData)
        {
            using var content = new MultipartFormDataContent();
            
            content.Add(new StringContent(_apiKey), "apikey");
            content.Add(new StringContent("2"), "OCREngine");
            content.Add(new StringContent("true"), "isTable");
            
            // Detect file type and set appropriate filename
            string fileName = "file.jpg"; // default
            if (imageData.Length > 4)
            {
                // Check PDF signature
                if (imageData[0] == 0x25 && imageData[1] == 0x50 && imageData[2] == 0x44 && imageData[3] == 0x46)
                {
                    fileName = "file.pdf";
                }
                // Check PNG signature
                else if (imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47)
                {
                    fileName = "file.png";
                }
            }
            
            content.Add(new ByteArrayContent(imageData), "file", fileName);

            var response = await _httpClient.PostAsync(_endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            
            try
            {
                var result = JsonSerializer.Deserialize<OcrSpaceResponse>(responseJson);

                if (result?.ParsedResults?.Length > 0)
                {
                    return result.ParsedResults[0].ParsedText ?? string.Empty;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"OCR JSON parsing failed: {ex.Message}");
                Console.WriteLine($"OCR Response: {responseJson}");
                
                // Try to extract text manually if JSON parsing fails
                try
                {
                    using var doc = JsonDocument.Parse(responseJson);
                    if (doc.RootElement.TryGetProperty("ParsedResults", out var parsedResults) &&
                        parsedResults.GetArrayLength() > 0)
                    {
                        var firstResult = parsedResults[0];
                        if (firstResult.TryGetProperty("ParsedText", out var parsedText))
                        {
                            return parsedText.GetString() ?? string.Empty;
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Manual JSON extraction also failed");
                }
            }

            return string.Empty;
        }
    }

    public class OcrSpaceResponse
    {
        public ParsedResult[]? ParsedResults { get; set; }
        public int OCRExitCode { get; set; }
        public bool IsErroredOnProcessing { get; set; }
        public object? ErrorMessage { get; set; } // Changed to object to handle different types
    }

    public class ParsedResult
    {
        public string? ParsedText { get; set; }
        public int ErrorCode { get; set; }
        public object? ErrorDetails { get; set; } // Changed to object to handle different types
    }
}