using MediatR;
using CabinCrewCareer.Domain.Wrappers;

namespace CabinCrewCareer.Application.Campaigns.Commands.CreateCampaignRound
{
    public class CreateCampaignRoundCommand : IRequest<Response<bool>>
    {
        public int CampaignId { get; set; }
        public string RoundName { get; set; }
        public string Description { get; set; }
        public int TargetQuantity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public CreateCampaignRoundCommand(int campaignId, string roundName, string description, int targetQuantity, DateTime? startDate, DateTime? endDate)
        {
            CampaignId = campaignId;
            RoundName = roundName;
            Description = description;
            TargetQuantity = targetQuantity;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
