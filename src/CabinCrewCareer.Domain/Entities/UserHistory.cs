using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class UserHistory
    {
        public int HistoryId { get; set; }

        public DateOnly ParticipatedDate { get; set; }

        public HistoryType Type { get; set; }

        // Foreign Keys
        public int? CampaignRoundId { get; set; }
        public virtual CampaignRound CampaignRound { get; set; } = null!;

        public int? UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Navigation Properties
    }
}
