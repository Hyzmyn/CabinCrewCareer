using CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignAndCreateRound;

namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateRejectedCampaign
{
    public class UpdateRejectedCampaignDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<RoundDto>? Rounds { get; set; }
    }
}