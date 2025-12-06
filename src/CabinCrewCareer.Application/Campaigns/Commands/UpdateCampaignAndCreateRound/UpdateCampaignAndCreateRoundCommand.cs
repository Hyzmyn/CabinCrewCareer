using MediatR;
using CabinCrewCareer.Domain.Wrappers;

namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignAndCreateRound
{
    public class UpdateCampaignAndCreateRoundCommand : IRequest<Response<bool>>
    {
        public int CampaignId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<RoundDto>? Rounds { get; set; }
        public List<RoundDateDto>? RoundDates { get; set; }

        public UpdateCampaignAndCreateRoundCommand(int campaignId, DateTime? startDate, DateTime? endDate, List<RoundDto>? rounds, List<RoundDateDto>? roundDates)
        {
            CampaignId = campaignId;
            StartDate = startDate;
            EndDate = endDate;
            Rounds = rounds;
            RoundDates = roundDates;
        }
    }
}
