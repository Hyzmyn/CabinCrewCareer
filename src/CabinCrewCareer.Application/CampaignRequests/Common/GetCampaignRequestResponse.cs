namespace CabinCrewCareer.Application.CampaignRequests.Common
{
    public class GetCampaignRequestResponse
    {
        public int RequestId { get; set; }

        public string CampaignName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int TargetQuantity { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? RejectReason { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string? PartnerName { get; set; }

        public string? DirectorName { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
