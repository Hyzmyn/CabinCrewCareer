namespace CabinCrewCareer.Application.CampaignRequests.Commands.UpdateRejectedCampaignRequest
{
    public class UpdateRejectedCampaignRequestDto
    {
        public string CampaignName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TargetQuantity { get; set; }
    }
}
