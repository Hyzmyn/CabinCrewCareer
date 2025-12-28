using CabinCrewCareer.Application.Common.Interfaces;
using CabinCrewCareer.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CabinCrewCareer.Infrastructure.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiService> _logger;
        private readonly string _cccdPrompt;
        private readonly string _toeicPrompt;

        public GeminiService(IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey is not configured");
            _model = configuration["Gemini:Model"] ?? "gemini-2.5-flash";
            _cccdPrompt = configuration["Gemini:Prompts:CCCD"] ?? throw new ArgumentNullException("Gemini:Prompts:CCCD is not configured");
            _toeicPrompt = configuration["Gemini:Prompts:TOEIC"] ?? throw new ArgumentNullException("Gemini:Prompts:TOEIC is not configured");
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(2) // Tăng timeout lên 2 phút
            };
            _logger = logger;
        }

        public async Task<CccdInfo> ExtractCccdInfoAsync(IFormFile imageFile)
        {
            var imageBase64 = await ConvertImageToBase64(imageFile);
            
            var prompt = _cccdPrompt;

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt },
                            new
                            {
                                inline_data = new
                                {
                                    mime_type = "image/jpeg",
                                    data = imageBase64
                                }
                            }
                        }
                    }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
 
            // Retry logic: thử 3 lần với exponential backoff
            HttpResponseMessage response = null;
            Exception lastException = null;
            
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    response = await _httpClient.PostAsync(url, content);
                    
                    // Nếu success, thoát loop
                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }
                    
                    // Nếu lỗi 429 (rate limit), đợi lâu hơn
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < 3)
                    {
                        var waitTime = attempt * 10000; // 10s, 20s, 30s
                        await Task.Delay(waitTime);
                        continue;
                    }
                    
                    // Các lỗi khác, break luôn
                    break;
                }
                catch (HttpRequestException ex) when (attempt < 3)
                {
                    lastException = ex;
                    await Task.Delay(2000);
                    continue;
                }
                catch (TaskCanceledException ex) when (attempt < 3)
                {
                    lastException = ex;
                    await Task.Delay(2000);
                    continue;
                }
            }
            
            if (response == null)
            {
                throw new Exception($"Failed to connect to Gemini API after 3 attempts. Last error: {lastException?.Message}");
            }
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                // Lỗi rate limit - thông báo rõ ràng
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    throw new Exception($"Gemini API rate limit exceeded. Vui lòng đợi 1-2 phút và thử lại. Chi tiết: {errorContent}");
                }
                
                throw new Exception($"Gemini API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("[CCCD] Raw Gemini API Response: {Response}", responseContent);
            
            var geminiResponse = JsonDocument.Parse(responseContent);

            var textResponse = geminiResponse.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            _logger.LogInformation("[CCCD] Extracted Text Response: {TextResponse}", textResponse);

            if (string.IsNullOrWhiteSpace(textResponse))
            {
                throw new Exception("Gemini API returned empty response");
            }

            // Extract JSON from markdown code blocks if present
            var jsonText = ExtractJsonFromMarkdown(textResponse);
            _logger.LogInformation("[CCCD] Extracted JSON: {JsonText}", jsonText);

            var cccdInfo = JsonSerializer.Deserialize<CccdInfo>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return cccdInfo ?? new CccdInfo();
        }

        public async Task<ToeicInfo> ExtractToeicInfoAsync(IFormFile imageFile)
        {
            var imageBase64 = await ConvertImageToBase64(imageFile);

            var prompt = _toeicPrompt;

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt },
                            new
                            {
                                inline_data = new
                                {
                                    mime_type = "image/jpeg",
                                    data = imageBase64
                                }
                            }
                        }
                    }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            
            // Retry logic: thử 3 lần với exponential backoff
            HttpResponseMessage response = null;
            Exception lastException = null;
            
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    response = await _httpClient.PostAsync(url, content);
                    
                    // Nếu success, thoát loop
                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }
                    
                    // Nếu lỗi 429 (rate limit), đợi lâu hơn
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < 3)
                    {
                        var waitTime = attempt * 10000; // 10s, 20s, 30s
                        await Task.Delay(waitTime);
                        continue;
                    }
                    
                    // Các lỗi khác, break luôn
                    break;
                }
                catch (HttpRequestException ex) when (attempt < 3)
                {
                    lastException = ex;
                    await Task.Delay(2000);
                    continue;
                }
                catch (TaskCanceledException ex) when (attempt < 3)
                {
                    lastException = ex;
                    await Task.Delay(2000);
                    continue;
                }
            }
            
            if (response == null)
            {
                throw new Exception($"Failed to connect to Gemini API after 3 attempts. Last error: {lastException?.Message}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                // Lỗi rate limit - thông báo rõ ràng
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    throw new Exception($"Gemini API rate limit exceeded. Vui lòng đợi 1-2 phút và thử lại. Chi tiết: {errorContent}");
                }
                
                throw new Exception($"Gemini API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("[TOEIC] Raw Gemini API Response: {Response}", responseContent);
            
            var geminiResponse = JsonDocument.Parse(responseContent);

            var textResponse = geminiResponse.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            _logger.LogInformation("[TOEIC] Extracted Text Response: {TextResponse}", textResponse);

            if (string.IsNullOrWhiteSpace(textResponse))
            {
                throw new Exception("Gemini API returned empty response for TOEIC");
            }

            // Extract JSON from markdown code blocks if present
            var jsonText = ExtractJsonFromMarkdown(textResponse);
            _logger.LogInformation("[TOEIC] Extracted JSON: {JsonText}", jsonText);

            var toeicInfo = JsonSerializer.Deserialize<ToeicInfo>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return toeicInfo ?? new ToeicInfo();
        }

        private async Task<string> ConvertImageToBase64(IFormFile imageFile)
        {
            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();
            
            // LUÔN LUÔN compress ảnh xuống max 600x600 với quality 80% để tránh timeout
            imageBytes = CompressImage(imageBytes, 600, 600);
            
            // Kiểm tra sau khi compress, nếu vẫn > 3MB thì giảm thêm
            const int maxSizeInBytes = 3 * 1024 * 1024; // 3MB
            if (imageBytes.Length > maxSizeInBytes)
            {
                // Compress thêm lần nữa với size nhỏ hơn
                imageBytes = CompressImage(imageBytes, 400, 400);
            }
            
            return Convert.ToBase64String(imageBytes);
        }

        private byte[] CompressImage(byte[] imageBytes, int maxWidth, int maxHeight)
        {
            try
            {
                using var inputStream = new MemoryStream(imageBytes);
                using var image = System.Drawing.Image.FromStream(inputStream);
                
                // Tính tỷ lệ resize
                double ratioX = (double)maxWidth / image.Width;
                double ratioY = (double)maxHeight / image.Height;
                double ratio = Math.Min(ratioX, ratioY);
                
                int newWidth = (int)(image.Width * ratio);
                int newHeight = (int)(image.Height * ratio);
                
                // Đảm bảo ít nhất cũng resize về maxWidth/maxHeight
                if (newWidth > maxWidth) newWidth = maxWidth;
                if (newHeight > maxHeight) newHeight = maxHeight;
                
                using var resized = new System.Drawing.Bitmap(newWidth, newHeight);
                using (var graphics = System.Drawing.Graphics.FromImage(resized))
                {
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                }
                
                using var outputStream = new MemoryStream();
                // Save với quality 80% (thấp hơn để giảm dung lượng)
                var encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                    .FirstOrDefault(e => e.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
                var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                    System.Drawing.Imaging.Encoder.Quality, 80L);
                
                resized.Save(outputStream, encoder, encoderParams);
                return outputStream.ToArray();
            }
            catch
            {
                // Nếu compress thất bại, thử resize đơn giản hơn
                try
                {
                    using var inputStream = new MemoryStream(imageBytes);
                    using var image = System.Drawing.Image.FromStream(inputStream);
                    using var resized = new System.Drawing.Bitmap(image, maxWidth, maxHeight);
                    using var outputStream = new MemoryStream();
                    resized.Save(outputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return outputStream.ToArray();
                }
                catch
                {
                    return imageBytes; // Worst case: trả về ảnh gốc
                }
            }
        }

        private string ExtractJsonFromMarkdown(string text)
        {
            // Remove markdown code blocks if present
            if (text.Contains("```json"))
            {
                var startIndex = text.IndexOf("```json") + 7;
                var endIndex = text.LastIndexOf("```");
                if (endIndex > startIndex)
                {
                    return text.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }
            else if (text.Contains("```"))
            {
                var startIndex = text.IndexOf("```") + 3;
                var endIndex = text.LastIndexOf("```");
                if (endIndex > startIndex)
                {
                    return text.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }

            return text.Trim();
        }
    }
}
