using CabinCrewCareer.Application.Common.Interfaces.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CabinCrewCareer.Infrastructure.Services
{
    public class UploadFileService : IUploadFileService
    {
        private readonly Cloudinary _cloudinary;

        public UploadFileService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account
                (
                    config.Value.CloudName,
                    config.Value.ApiKey,
                    config.Value.ApiSecret
                );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<string> CloudinaryStorage(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream)
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            return result.SecureUrl.AbsoluteUri;
        }

        #region Firebase
        //public async Task<string> UploadImage(IFormFile file)
        //{
        //    var _apiKey = _configuration["Firebase:ApiKey"];
        //    var _storage = _configuration["Firebase:Storage"];

        //    var storage = new FirebaseStorage(_storage);

        //    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        //    var fileExtension = Path.GetExtension(file.FileName);
        //    string folderName = fileExtension switch
        //    {
        //        ".jpg" or ".jpeg" or ".png" => "images",
        //        ".docx" => "docx",
        //        ".ppt" or ".pptx" => "ppt",
        //        ".mp4" or ".mov" => "videos",
        //        _ => "other",
        //    };
        //    using var stream = file.OpenReadStream();
        //    var storageReference = storage.Child(folderName).Child(fileName);
        //    await storageReference.PutAsync(stream);
        //    return await storageReference.GetDownloadUrlAsync();
        //}
        #endregion
    }
}
