using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    public class CampaignRoundRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<CampaignRound>(dbContext, logger), ICampaignRoundRepository
    {
        public async Task<List<CampaignRound>> GetUserHistoryAsync()
        {
            var campaigns = await _dbContext.CampaignRounds
                                   .Include(r => r.Campaign)
                                           .ThenInclude(r => r.CampaignRequest)
                                                       .ThenInclude(r => r.AirlinePartner)
                                   .Include(r => r.Rounds)
                                   .Where(r => r.RoundStatus == RoundStatus.Ended).ToListAsync();

            return campaigns;
        }

        public async Task<List<CampaignRound>> GetUserOngoingCampaignAsync()
        {
            var campaign = _dbContext.CampaignRounds
                            .Include(r => r.Campaign)
                                    .ThenInclude(r => r.CampaignRequest)
                                                .ThenInclude(r => r.AirlinePartner)
                            .Include(r => r.Rounds)
                            .Where(r => r.RoundStatus == RoundStatus.Ongoing);

            return await campaign.ToListAsync();
        }
    }
}
