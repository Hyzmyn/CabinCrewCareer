namespace CabinCrewCareer.Domain.Entities
{
    public class ScoringCriteria
    {
        public int ScoringCriteriaId { get; set; }

        public string Title { get; set; } = string.Empty;

        // Foreign Keys

        // Navigation Properties
        public virtual ICollection<ScoringCriteriaItem> ScoringCriteriaItems { get; set; } = [];
    }
}
