using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities.Base.Implementations;

namespace CabinCrewCareer.Domain.Entities
{
    public class CampaignRequest : AuditableEntity
    {
        public int RequestId { get; set; }

        public string CampaignName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public string JobRequirement { get; set; } = string.Empty;

        public int TargetQuantity { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public CampaignType RequestType { get; set; }

        public string RejectReason { get; set; } = string.Empty;

        public int RejectedCount { get; set; } = 0;

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        public DateOnly? DueDate { get; set; }

        // Foreign Keys
        public int? PartnerId { get; set; }
        public virtual AirlinePartner AirlinePartner { get; set; } = null!;

        public int? DirectorId { get; set; }
        public virtual User Director { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<Campaign> Campaigns { get; set; } = [];
    }
}
