using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CabinCrewCareer.Application.Common.Interfaces
{
    public interface IOcrService
    {
        Task<string> ExtractTextFromImageAsync(IFormFile imageFile);
    }
}