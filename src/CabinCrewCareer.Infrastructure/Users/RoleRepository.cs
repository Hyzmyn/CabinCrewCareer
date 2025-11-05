using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Users
{
    public class RoleRepository(
        CabinCrewCareerDbContext dbContext,
        ILogger logger) : Repository<Role>(dbContext, logger), IRoleRepository
    {
    }
}
