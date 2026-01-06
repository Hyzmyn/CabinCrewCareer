namespace CabinCrewCareer.Domain.Entities
{
    public class ScoringCriteriaDetail
    {
        public int ScoringCriteriaDetailId { get; set; }

        public string DetailText { get; set; } = string.Empty;

        public bool Status { get; set; } = false;

        public DateOnly? EffectiveDate { get; set; }

        // Foreign Keys
        public int? ScoringCriteriaItemId { get; set; }
        public virtual ScoringCriteriaItem ScoringCriteriaItem { get; set; } = null!;
    }
}
