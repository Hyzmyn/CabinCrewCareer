using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    using CabinCrewCareer.Infrastructure.Common.Persistence;

    public class CampaignRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<Campaign>(dbContext, logger), ICampaignRepository
    {
        // Add custom methods if needed
    }
}
