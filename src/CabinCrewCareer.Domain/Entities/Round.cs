using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class Round
    {
        public int RoundId { get; set; }

        public string Notes { get; set; } = string.Empty;

        public RoundType RoundType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        // Foreign Keys
        public int? CampaignRoundId { get; set; }
        public virtual CampaignRound CampaignRound { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<UserApplication> Applications { get; set; } = [];
    }
}
