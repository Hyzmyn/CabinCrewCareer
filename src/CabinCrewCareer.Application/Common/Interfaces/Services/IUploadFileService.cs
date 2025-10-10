using Microsoft.AspNetCore.Http;

namespace CabinCrewCareer.Application.Common.Interfaces.Services
{
    public interface IUploadFileService
    {
        Task<string> CloudinaryStorage(IFormFile file);
    }
}
