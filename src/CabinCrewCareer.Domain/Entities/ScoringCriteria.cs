namespace CabinCrewCareer.Domain.Entities
{
    public class ScoringCriteria
    {
        public int ScoringCriteriaId { get; set; }

        public string Title { get; set; } = string.Empty;

        public bool Status { get; set; } = false;

        public DateOnly? EffectiveDate { get; set; }

        // Foreign Keys

        // Navigation Properties
        public virtual ICollection<ScoringCriteriaItem> ScoringCriteriaItems { get; set; } = [];
    }
}
