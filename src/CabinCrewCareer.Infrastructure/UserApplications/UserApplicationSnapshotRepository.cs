using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.UserApplications
{
    public class UserApplicationSnapshotRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<UserApplicationSnapshot>(dbContext, logger), IUserApplicationSnapshotRepository
    {
    }
}
