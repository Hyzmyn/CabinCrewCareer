using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    using CabinCrewCareer.Infrastructure.Common.Persistence;

    public class CampaignRepository : Repository<Campaign>, ICampaignRepository
    {
        public CampaignRepository(CabinCrewCareer.Infrastructure.Common.Persistence.CabinCrewCareerDbContext dbContext, ILogger logger)
            : base(dbContext, logger)
        {
        }
        // Add custom methods if needed
    }
}
