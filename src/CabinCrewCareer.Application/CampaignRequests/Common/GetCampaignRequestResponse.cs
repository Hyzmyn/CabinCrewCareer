namespace CabinCrewCareer.Application.CampaignRequests.Common
{
    public class GetCampaignRequestResponse
    {
        public int RequestId { get; set; }

        public string CampaignName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public string JobRequirement { get; set; } = string.Empty;

        public int TargetQuantity { get; set; }

        public string RequestType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? RejectReason { get; set; }

        public string? ApprovedAt { get; set; }

        public string? RejectedAt { get; set; }

        public string? PartnerName { get; set; }

        public string? DirectorName { get; set; }

        public string? CreatedAt { get; set; }
    }
}
