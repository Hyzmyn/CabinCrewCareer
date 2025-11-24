namespace CabinCrewCareer.Domain.Entities
{
    public class InterviewResult
    {
        public int InterviewResultId { get; set; }

        public int Score { get; set; } = 0;

        public string Comment { get; set; } = string.Empty;

        // Foreign Keys
        public int? EvaluationId { get; set; }
        public virtual EvaluationForm EvaluationForm { get; set; } = null!;

        public int? InterviewCriteriaItemId { get; set; }
        public virtual InterviewCriteriaItem InterviewCriteriaItem { get; set; } = null!;
    }
}
