using Astra.Dtos.Responses.Xion;

namespace Astra.Services.Http {
    public class XionClient {
        private readonly HttpClient _client;
        public XionClient(HttpClient client) {
            _client = client;
            _client.BaseAddress = new Uri("https://xion-api.com");
            _client.Timeout = TimeSpan.FromMinutes(1);
        }

        public async Task<GeneratedWalletResponse?> GenerateWallet(string hexPrivateKey) {
            var response = await _client.GetAsync("/api");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GeneratedWalletResponse>();
        }
    }
}