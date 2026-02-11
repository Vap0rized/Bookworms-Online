using System.Net.Http.Json;

namespace Bookworms_Online.Services
{
    public class RecaptchaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RecaptchaService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<bool> VerifyAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var secret = _configuration["Recaptcha:SecretKey"];
            if (string.IsNullOrWhiteSpace(secret))
            {
                return false;
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}", null);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();
            return result?.Success == true && result.Score >= 0.5;
        }

        private sealed class RecaptchaResponse
        {
            public bool Success { get; set; }
            public double Score { get; set; }
        }
    }
}
