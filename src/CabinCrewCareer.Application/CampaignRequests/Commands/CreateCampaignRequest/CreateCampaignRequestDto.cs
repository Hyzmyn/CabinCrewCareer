using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Application.CampaignRequests.Commands.CreateCampaignRequest
{
    public class CreateCampaignRequestDto
    {
        public string CampaignName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public string JobRequirement { get; set; } = string.Empty;

        public int TargetQuantity { get; set; }

        public CampaignType RequestType { get; set; }
            public DateOnly? DueDate { get; set; }
    }
}
