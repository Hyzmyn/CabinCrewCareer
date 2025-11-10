namespace CabinCrewCareer.Application.Campaigns.Queries.GetCampaignList
{
    public class GetCampaignListDto
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TargetQuantity { get; set; }
        public string CampaignType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}
