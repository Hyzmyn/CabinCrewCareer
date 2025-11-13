using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class Campaign
    {
        public int CampaignId { get; set; }

        public string CampaignName { get; set; } = string.Empty;

        public CampaignType CampaignType { get; set; }

        public string Description { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public string JobRequirement { get; set; } = string.Empty;

        public int TargetQuantity { get; set; }

        public int ActualQuantiy { get; set; }

        public CampaignStatus CampaignStatus { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string? ReviewedBy { get; set; } = string.Empty;

        public string? RejectedReason { get; set; } = string.Empty;

        public int RejectedCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Foreign Keys
        public int? SeniorId { get; set; }
        public virtual User SeniorRecruiter { get; set; } = null!;

        public int? RequestId { get; set; }
        public virtual CampaignRequest CampaignRequest { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<CampaignRound> CampaignRounds { get; set; } = [];
        public virtual ICollection<CampaignAssignment> Assignments { get; set; } = [];
    }
}
