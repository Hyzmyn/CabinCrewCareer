using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class RoundActivity
    {
        public int ActivityId { get; set; }

        public ActivityStatus Status { get; set; }

        // Foreign Keys
        public int? RoundId { get; set; }
        public virtual Round Round { get; set; } = null!;

        public int? UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int? ApplicationSnapshotId { get; set; }
        public virtual UserApplicationSnapshot UserApplicationSnapshot { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<EvaluationForm> EvaluationForms { get; set; } = [];
    }
}
