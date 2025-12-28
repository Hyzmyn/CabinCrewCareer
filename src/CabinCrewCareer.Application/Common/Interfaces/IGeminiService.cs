using CabinCrewCareer.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CabinCrewCareer.Application.Common.Interfaces
{
    public interface IGeminiService
    {
        Task<CccdInfo> ExtractCccdInfoAsync(IFormFile imageFile);
        Task<ToeicInfo> ExtractToeicInfoAsync(IFormFile imageFile);
    }
}
