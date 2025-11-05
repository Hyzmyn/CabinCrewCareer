namespace CabinCrewCareer.Application.CampaignRequests.Commands.UpdateCampaignRequestStatus
{
    public class UpdateCampaignRequestStatusDto
    {
        public int Status { get; set; } // 2: Approved, 3: Rejected

        public string? RejectReason { get; set; }
    }
}
