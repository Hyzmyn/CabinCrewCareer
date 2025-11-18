namespace CabinCrewCareer.Domain.Entities
{
    public class InterviewCriteriaItem
    {
        public int InterviewCriteriaItemId { get; set; }

        public string Criteria { get; set; } = string.Empty;

        // Foreign Keys
        public int? InterviewCriteriaId { get; set; }
        public virtual InterviewCriteria InterviewCriteria { get; set; } = null!;

        // Navigation Properties
    }
}
