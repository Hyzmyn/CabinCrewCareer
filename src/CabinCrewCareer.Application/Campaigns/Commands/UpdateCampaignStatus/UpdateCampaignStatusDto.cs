namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignStatus
{
    public class UpdateCampaignStatusDto
    {
        public int Status { get; set; }
        public string? RejectReason { get; set; }
    }
}