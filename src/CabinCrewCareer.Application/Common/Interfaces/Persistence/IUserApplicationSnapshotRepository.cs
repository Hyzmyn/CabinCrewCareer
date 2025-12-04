using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface IUserApplicationSnapshotRepository : IRepository<UserApplicationSnapshot>
    {
        Task<UserApplicationSnapshot?> GetUserApplicationSnapshotByIdAsync(int id);

        Task<bool> UpdateApplicationStatusAsync(int id, ApplicationStatus status);
    }
}
