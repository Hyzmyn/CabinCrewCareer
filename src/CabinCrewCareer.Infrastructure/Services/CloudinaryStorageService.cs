using CabinCrewCareer.Application.Common.Interfaces.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace CabinCrewCareer.Infrastructure.Services
{
    public class CloudinaryStorageService : IFirebaseStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryStorageService(IConfiguration configuration)
        {
            var cloudName = configuration["CloudinarySettings:CloudName"];
            var apiKey = configuration["CloudinarySettings:ApiKey"];
            var apiSecret = configuration["CloudinarySettings:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadAudioFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                // Generate unique filename with timestamp
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var extension = Path.GetExtension(fileName);
                var uniqueFileName = $"{timestamp}_{fileNameWithoutExt}{extension}";

                var uploadParams = new RawUploadParams()
                {
                    File = new FileDescription(fileName, fileStream),
                    PublicId = $"audio-tests/{fileNameWithoutExt}_{timestamp}",
                    Folder = "audio-tests"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");
                }

                // Return secure URL
                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload file to Cloudinary: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Extract public_id from URL
                var uri = new Uri(fileUrl);
                var segments = uri.AbsolutePath.Split('/');
                
                // Find the public_id (everything after /upload/)
                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex >= 0 && uploadIndex < segments.Length - 1)
                {
                    var publicIdParts = segments.Skip(uploadIndex + 2).ToArray();
                    var publicId = string.Join("/", publicIdParts);
                    
                    // Remove file extension
                    publicId = Path.GetFileNameWithoutExtension(publicId);
                    
                    var deletionParams = new DeletionParams(publicId)
                    {
                        ResourceType = ResourceType.Raw
                    };

                    var result = await _cloudinary.DestroyAsync(deletionParams);
                    return result.Result == "ok";
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
