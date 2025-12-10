using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.CampaignRequests
{
    public class RequirementRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<Requirement>(dbContext, logger), IRequirementRepository
    {
    }
}
