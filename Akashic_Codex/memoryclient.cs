using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NyxHeart.Runtime
{
    public class MemoryClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public MemoryClient(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();
        }

        public async Task<bool> StoreMemoryAsync(string content, string[] tags = null, string tier = "warm", bool useVector = false)
        {
            var payload = new
            {
                content = content,
                tags = tags,
                tier = tier,
                use_vector = useVector
            };

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/memory/store_memory",
                new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            );

            return response.IsSuccessStatusCode;
        }

        public async Task<string> RecallMemoryAsync(string query, string tier = "warm", int topK = 5, bool useVector = false)
        {
            var payload = new
            {
                query = query,
                tier = tier,
                top_k = topK,
                use_vector = useVector
            };

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/memory/recall",
                new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            );

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SearchMemoryAsync(string query, bool useVector = false, bool includeAllTiers = true)
        {
            var payload = new
            {
                query = query,
                use_vector = useVector,
                include_all_tiers = includeAllTiers
            };

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/memory/search",
                new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            );

            return await response.Content.ReadAsStringAsync();
        }
    }
}
