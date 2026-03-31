using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Patient_mgt.Infrastructure.RAG
{
    public class RagResponse
    {
        [JsonProperty("query")]
        public string Query { get; set; } = string.Empty;

        [JsonProperty("results")]
        public List<RagResult> Results { get; set; } = new();
    }

    public class RagResult
    {
        [JsonProperty("document")]
        public string Document { get; set; } = string.Empty;

        [JsonProperty("source")]
        public string Source { get; set; } = string.Empty;

        [JsonProperty("chunk_id")]
        public int ChunkId { get; set; }
    }

    public class RagService : IRagService
    {
        private readonly HttpClient _httpClient;

        public RagService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> GetRelevantChunks(string query)
        {
            var response = await _httpClient.GetAsync(
                $"http://127.0.0.1:8000/query?q={Uri.EscapeDataString(query)}"
            );

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RagResponse>(json);
            return result?.Results?.Select(r => r.Document).ToList() ?? new List<string>();
        }
    }
}
