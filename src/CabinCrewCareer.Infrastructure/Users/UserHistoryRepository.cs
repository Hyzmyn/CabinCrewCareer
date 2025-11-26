using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Users
{
    public class UserHistoryRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<UserHistory>(dbContext, logger), IUserHistoryRepository
    {
    }
}
