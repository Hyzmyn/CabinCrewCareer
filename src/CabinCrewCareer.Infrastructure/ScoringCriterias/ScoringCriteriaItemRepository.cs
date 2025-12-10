using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.ScoringCriterias
{
    public class ScoringCriteriaItemRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<ScoringCriteriaItem>(dbContext, logger), IScoringCriteriaItemRepository
    {
    }
}
