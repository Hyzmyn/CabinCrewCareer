using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Users
{
    public class UserHistoryRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<UserHistory>(dbContext, logger), IUserHistoryRepository
    {
        public async Task<List<UserHistory>> GetUserCompletedCampaignsAsync(int userId)
        {
            return await _dbContext.UserHistories
                .Where(x => x.UserId == userId && x.Type != HistoryType.Ongoing)
                .Select(x => new UserHistory
                {
                    CampaignRoundId = x.CampaignRoundId,
                    Type = x.Type,
                    ParticipatedDate = x.ParticipatedDate,
                    CampaignRound = new CampaignRound
                    {
                        RoundName = x.CampaignRound.RoundName,
                        Description = x.CampaignRound.Description,
                        Campaign = new Campaign
                        {
                            CampaignName = x.CampaignRound.Campaign.CampaignName,
                            CampaignType = x.CampaignRound.Campaign.CampaignType,
                            CampaignRequest = new CampaignRequest
                            {
                                AirlinePartner = new AirlinePartner
                                {
                                    PartnerName = x.CampaignRound.Campaign.CampaignRequest.AirlinePartner.PartnerName
                                }
                            }
                        },
                        Rounds = x.CampaignRound.Rounds.Select(z => new Round
                        {
                            RoundId = z.RoundId,
                            RoundType = z.RoundType
                        }).ToList()
                    }
                }).ToListAsync();
        }

        public async Task<UserHistory> GetUserOngoingCampaignAsync(int userId)
        {
            var result = await _dbContext.UserHistories
                .Where(x => x.UserId == userId && x.Type == HistoryType.Ongoing)
                .Select(x => new UserHistory
                {
                    CampaignRoundId = x.CampaignRoundId,
                    Type = x.Type,
                    ParticipatedDate = x.ParticipatedDate,
                    CampaignRound = new CampaignRound
                    {
                        RoundName = x.CampaignRound.RoundName,
                        Description = x.CampaignRound.Description,
                        Campaign = new Campaign
                        {
                            CampaignName = x.CampaignRound.Campaign.CampaignName,
                            CampaignType = x.CampaignRound.Campaign.CampaignType,
                            CampaignRequest = new CampaignRequest
                            {
                                AirlinePartner = new AirlinePartner
                                {
                                    PartnerName = x.CampaignRound.Campaign.CampaignRequest.AirlinePartner.PartnerName
                                }
                            }
                        },
                        Rounds = x.CampaignRound.Rounds.Select(z => new Round
                        {
                            RoundId = z.RoundId,
                            RoundType = z.RoundType
                        }).ToList()
                    }
                }).FirstOrDefaultAsync();

            if (result is null) return new UserHistory();

            return result;
        }

        public async Task<bool> HasAppliedToCampainRound(int userId, int campaignRoundId)
        {
            var result = await _dbContext.UserHistories.Where(x => x.UserId == userId && x.CampaignRoundId == campaignRoundId).Select(x => x.HistoryId).FirstOrDefaultAsync();

            if (result > 0) return true;

            return false;
        }

        public async Task<bool> HasOngoingCampainRound(int userId)
        {
            var result = await _dbContext.UserHistories.Where(x => x.UserId == userId && x.Type == HistoryType.Ongoing).Select(x => x.HistoryId).FirstOrDefaultAsync();

            if (result > 0) return true;

            return false;
        }
    }
}
