using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface IUserApplicationRepository : IRepository<UserApplication>
    {
        //Task<UserApplication> CreateUserApplicationAsync(UserApplication application, List<Document> documents);

        //Task<UserApplication?> GetUserApplicationByIdAsync(int id);

        //Task<UserApplication?> GetUserApplicationByUserIdAsync(int userId);

        //Task<bool> UpdateApplicationStatusAsync(int id, ApplicationStatus status);
    }
}
