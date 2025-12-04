namespace CabinCrewCareer.Application.Campaigns.Queries.GetCampaignDetail
{
    public class GetCampaignDetailDto
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public string JobRequirement { get; set; } = string.Empty;
        public int TargetQuantity { get; set; }
        public string CampaignType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? PartnerName { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? ApprovedAt { get; set; }
        public string? RejectedAt { get; set; }
        public string? ReviewedBy { get; set; }
        public string? RejectedReason { get; set; }
        public int RejectedCount { get; set; }
        public string? CreatedAt { get; set; }
        public List<RoundDto> Rounds { get; set; } = new();
    }

    public class RoundDto
    {
        public int CampaignRoundId { get; set; }
        public string RoundName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TargetQuantity { get; set; }
        public int ActualQuantiy { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        // Screening round specific dates (optional, only set for screening rounds)
        public string? ScreeningStartDate { get; set; }
        public string? ScreeningEndDate { get; set; }
    }
}
