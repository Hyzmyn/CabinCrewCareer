using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class EvaluationForm
    {
        public int EvaluationId { get; set; }

        public EvaluationType Type { get; set; }

        public DateTime EvaluatedDate { get; set; }

        public bool IsPassed { get; set; } = false;

        // Foreign Keys
        public int? ActivityId { get; set; }
        public virtual RoundActivity RoundActivity { get; set; } = null!;

        public int? ExaminerId { get; set; }
        public virtual User Examiner { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<AppearanceResult> AppearanceResults { get; set; } = [];

        public virtual ICollection<InterviewResult> InterviewResults { get; set; } = [];
    }
}
