namespace CabinCrewCareer.Domain.Entities
{
    public class InterviewCriteriaItem
    {
        public int InterviewCriteriaItemId { get; set; }

        public string Criteria { get; set; } = string.Empty;

        public bool Status { get; set; } = false;

        public DateOnly? EffectiveDate { get; set; }

        // Foreign Keys
        public int? InterviewCriteriaId { get; set; }
        public virtual InterviewCriteria InterviewCriteria { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<InterviewResult> InterviewResults { get; set; } = [];
    }
}
