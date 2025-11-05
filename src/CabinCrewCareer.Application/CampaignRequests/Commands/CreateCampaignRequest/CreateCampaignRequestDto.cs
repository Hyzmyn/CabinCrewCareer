namespace CabinCrewCareer.Application.CampaignRequests.Commands.CreateCampaignRequest
{
    public class CreateCampaignRequestDto
    {
        public string CampaignName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int TargetQuantity { get; set; }
    }
}
