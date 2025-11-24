namespace CabinCrewCareer.Domain.Entities
{
    public class AppearanceResult
    {
        public int AppearanceResultId { get; set; }

        public bool IsPassed { get; set; } = false;

        // Foreign Keys
        public int? EvaluationId { get; set; }
        public virtual EvaluationForm EvaluationForm { get; set; } = null!;

        public int? ScoringCriteriaItemId { get; set; }
        public virtual ScoringCriteriaItem ScoringCriteriaItem { get; set; } = null!;
    }
}
