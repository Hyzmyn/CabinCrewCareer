using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    public class RoundRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<Round>(dbContext, logger), IRoundRepository
    {
    }
}
