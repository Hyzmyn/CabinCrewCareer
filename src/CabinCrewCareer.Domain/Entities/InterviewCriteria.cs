namespace CabinCrewCareer.Domain.Entities
{
    public class InterviewCriteria
    {
        public int InterviewCriteriaId { get; set; }

        public string Title { get; set; } = string.Empty;

        // Foreign Keys

        // Navigation Properties
        public virtual ICollection<InterviewCriteriaItem> InterviewCriteriaItems { get; set; } = [];
    }
}
