using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GIBS.Module.IPGuard.Services
{
    public class IpApiGeoLocationService
    {
        public const string LookupError = "Error Looking Up Country";

        private readonly IHttpClientFactory _httpClientFactory;

        public IpApiGeoLocationService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetCountryAsync(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return LookupError;
            }

            try
            {
                using var client = _httpClientFactory.CreateClient();
                using var response = await client.GetAsync($"http://ip-api.com/json/{Uri.EscapeDataString(ipAddress)}?fields=status,country");
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<IpApiResponse>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web));

                return !string.IsNullOrWhiteSpace(result?.Country) && string.Equals(result.Status, "success", StringComparison.OrdinalIgnoreCase)
                    ? result.Country
                    : LookupError;
            }
            catch
            {
                return LookupError;
            }
        }

        private class IpApiResponse
        {
            public string Status { get; set; }
            public string Country { get; set; }
        }
    }
}
