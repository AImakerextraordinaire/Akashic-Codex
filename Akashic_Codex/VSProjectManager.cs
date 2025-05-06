using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;

namespace CodemuseAI
{
    public static class VSProjectManager
    {
        private static readonly HttpClient client = new HttpClient();

        // 🔹 Define API Endpoint and Auth Token Retrieval
        private static readonly string WORKER_URL = "https://akashic-key-worker.darren-wesley13.workers.dev/get-akashic-key";
        private static readonly string WORKER_AUTH_TOKEN = Environment.GetEnvironmentVariable("WORKER_AUTH_TOKEN");
        private static readonly string API_BASE_URL = "https://remote.neural-forge.org/";
        private static readonly string API_KEY = Environment.GetEnvironmentVariable("API_KEY");using System;
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

    private static string cachedApiKey = null; // ✅ Cache the API key for reuse

    public static async Task InitializeAsync(AsyncPackage package)
    {
        await Task.CompletedTask; // Placeholder for future initialization logic
    }

    public static async Task<string> SendApiRequestAsync(string endpoint, string jsonPayload = null)
    {
        if (string.IsNullOrEmpty(API_KEY))
        {
            Console.WriteLine("⚠️ API Key is missing from environment variables!");
            return "⚠️ API Key retrieval failed.";
        }

        var url = $"{API_BASE_URL}{endpoint}";
        HttpRequestMessage request = new HttpRequestMessage(
            jsonPayload != null ? HttpMethod.Post : HttpMethod.Get, url
        );
        request.Headers.Add("X-API-Key", API_KEY); // ✅ Dynamically attach API key

        if (jsonPayload != null)
        {
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        }

        using (HttpResponseMessage response = await client.SendAsync(request))
        {
            return await response.Content.ReadAsStringAsync();
        }
    }

    private static async Task<string> GetApiKeyAsync()
    {
        if (!string.IsNullOrEmpty(cachedApiKey))
            return cachedApiKey;

        if (string.IsNullOrEmpty(WORKER_AUTH_TOKEN))
            throw new Exception("⚠️ Missing WORKER_AUTH_TOKEN in environment variables!");

        using (var request = new HttpRequestMessage(HttpMethod.Get, WORKER_URL))
        {
            request.Headers.Add("Authorization", $"Bearer {WORKER_AUTH_TOKEN}");

            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic jsonData = JsonConvert.DeserializeObject(jsonResponse);
                    cachedApiKey = jsonData.api_key;
                    return cachedApiKey;
                }
                else
                {
                    throw new Exception($"❌ API Key Fetch Failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
        }
    }
}
}
