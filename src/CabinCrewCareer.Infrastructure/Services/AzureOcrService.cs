using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CabinCrewCareer.Application.Common.Interfaces;

namespace CabinCrewCareer.Infrastructure.Services
{
    public class AzureOcrService : IOcrService
    {
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public AzureOcrService(IConfiguration configuration)
        {
            _endpoint = configuration["AzureOcr:Endpoint"];
            _apiKey = configuration["AzureOcr:ApiKey"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
        }

        public async Task<string> ExtractTextFromImageAsync(IFormFile imageFile)
        {
            using var stream = imageFile.OpenReadStream();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);

            var url = $"{_endpoint}/vision/v3.2/ocr?language=unk&detectOrientation=true";
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            // Parse JSON để lấy toàn bộ text
            var allText = new System.Text.StringBuilder();
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(result);
                if (doc.RootElement.TryGetProperty("regions", out var regions))
                {
                    foreach (var region in regions.EnumerateArray())
                    {
                        if (region.TryGetProperty("lines", out var lines))
                        {
                            foreach (var line in lines.EnumerateArray())
                            {
                                if (line.TryGetProperty("words", out var words))
                                {
                                    foreach (var word in words.EnumerateArray())
                                    {
                                        if (word.TryGetProperty("text", out var text))
                                        {
                                            allText.Append(text.GetString());
                                            allText.Append(' ');
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Nếu lỗi parse, trả về kết quả gốc
                return result;
            }
            return allText.ToString();
        }
    }
}
