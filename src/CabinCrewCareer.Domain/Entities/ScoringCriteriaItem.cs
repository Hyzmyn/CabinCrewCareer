namespace CabinCrewCareer.Domain.Entities
{
    public class ScoringCriteriaItem
    {
        public int ScoringCriteriaItemId { get; set; }

        public string ItemType { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string EnglishText { get; set; } = string.Empty;

        // Foreign Keys
        public int? ScoringCriteriaId { get; set; }
        public virtual ScoringCriteria ScoringCriteria { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<ScoringCriteriaDetail> ScoringCriteriaDetails { get; set; } = [];
    }
}
