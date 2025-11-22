using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface ICampaignRoundRepository : IRepository<CampaignRound>
    {
        Task<List<CampaignRound>> GetUserHistoryAsync();

        Task<List<CampaignRound>> GetUserOngoingCampaignAsync();
    }
}
