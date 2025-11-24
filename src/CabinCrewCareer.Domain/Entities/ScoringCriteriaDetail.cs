namespace CabinCrewCareer.Domain.Entities
{
    public class ScoringCriteriaDetail
    {
        public int ScoringCriteriaDetailId { get; set; }

        public string DetailText { get; set; } = string.Empty;

        // Foreign Keys
        public int? ScoringCriteriaItemId { get; set; }
        public virtual ScoringCriteriaItem ScoringCriteriaItem { get; set; } = null!;

        // Navigation Property
        public virtual ICollection<AppearanceResult> AppearanceResults { get; set; } = [];
    }
}
