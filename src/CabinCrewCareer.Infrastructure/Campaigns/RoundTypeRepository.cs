using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    public class RoundTypeRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<TypeRound>(dbContext, logger), IRoundTypeRepository
    {
    }
}
