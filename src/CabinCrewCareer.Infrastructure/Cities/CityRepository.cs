using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Cities
{
    public class CityRepository(
        CabinCrewCareerDbContext dbContext,
        ILogger logger) : Repository<City>(dbContext, logger), ICityRepository
    {
    }
}
