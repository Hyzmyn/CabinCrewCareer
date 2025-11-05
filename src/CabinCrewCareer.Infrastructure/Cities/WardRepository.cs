using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Cities
{
    public class WardRepository(
        CabinCrewCareerDbContext dbContext,
        ILogger logger) : Repository<Ward>(dbContext, logger), IWardRepository
    {
    }
}
