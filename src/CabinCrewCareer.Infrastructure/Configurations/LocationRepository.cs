using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Configurations
{
    public class LocationRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<Location>(dbContext, logger), ILocationRepository
    {
    }
}
