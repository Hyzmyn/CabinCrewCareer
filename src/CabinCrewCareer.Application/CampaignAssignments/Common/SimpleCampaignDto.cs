namespace CabinCrewCareer.Application.CampaignAssignments.Common
{
    public class SimpleCampaignDto
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string CampaignType { get; set; } = string.Empty;
    }
}