using Patient_mgt.DTOs;
using System.Text.Json;

namespace Patient_mgt.Infrastructure
{
    public class IcdService : IIcdService
    {
        private readonly HttpClient _httpClient;

        public IcdService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<IcdCodeDTO>> SearchIcdCodes(string query)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://clinicaltables.nlm.nih.gov/api/icd10cm/v3/search?sf=code,name&terms={Uri.EscapeDataString(query)}&maxList=10");
                
                if (!response.IsSuccessStatusCode)
                    return new List<IcdCodeDTO>();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var jsonArray = JsonDocument.Parse(jsonContent).RootElement;
                
                if (jsonArray.GetArrayLength() < 4)
                    return new List<IcdCodeDTO>();

                var codes = jsonArray[3].EnumerateArray();
                var results = new List<IcdCodeDTO>();

                foreach (var code in codes)
                {
                    if (code.GetArrayLength() >= 2)
                    {
                        results.Add(new IcdCodeDTO
                        {
                            Code = code[0].GetString() ?? "",
                            Description = code[1].GetString() ?? ""
                        });
                    }
                }

                return results;
            }
            catch
            {
                return new List<IcdCodeDTO>();
            }
        }
    }
}