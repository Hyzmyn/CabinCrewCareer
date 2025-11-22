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
                            .Select(c =>
                            new CampaignRound
                            {
                                CampaignRoundId = c.CampaignRoundId,
                                RoundName = c.RoundName,
                                RoundStatus = c.RoundStatus,
                                Campaign = new Campaign
                                {
                                    CampaignName = c.Campaign.CampaignName,
                                    CampaignRequest = new CampaignRequest
                                    {
                                        AirlinePartner = new AirlinePartner
                                        {
                                            PartnerName = c.Campaign.CampaignRequest.AirlinePartner.PartnerName
                                        }
                                    },
                                    Description = c.Campaign.Description
                                },
                                Rounds = c.Rounds.Select(r => new Round
                                {
                                    RoundId = r.RoundId,
                                    RoundType = r.RoundType,
                                    //RoundActivities = r.RoundActivities.Select(r => new RoundActivity
                                    //{
                                    //    Status = r.Status
                                    //}).Where(r => r.UserId == userId).ToList()
                                }).ToList()
                            }).Where(r => r.RoundStatus == RoundStatus.Ended).ToListAsync();

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
