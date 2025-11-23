using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface IUserApplicationRepository
    {
        Task<UserApplication> CreateUserApplicationAsync(UserApplication application, List<Document> documents);
        Task<UserApplication?> GetUserApplicationByIdAsync(int id);
        Task<UserApplication?> GetUserApplicationByUserIdAsync(int userId);
        Task<bool> UpdateApplicationStatusAsync(int id, ApplicationStatus status);
    }
}
