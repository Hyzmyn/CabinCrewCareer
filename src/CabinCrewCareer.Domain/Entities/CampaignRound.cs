using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class CampaignRound
    {
        public int CampaignRoundId { get; set; }

        public string RoundName { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public int TargetQuantity { get; set; }

        public int ActualQuantiy { get; set; }

        public RoundStatus RoundStatus { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign Keys
        public int? CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<Round> Rounds { get; set; } = [];
        public virtual ICollection<UserHistory> Histories { get; set; } = [];
    }
}
