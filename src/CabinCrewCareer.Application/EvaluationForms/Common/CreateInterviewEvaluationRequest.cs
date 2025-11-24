namespace CabinCrewCareer.Application.EvaluationForms.Common
{
    public class CreateInterviewEvaluationRequest
    {
        public int Score { get; set; } = 0;

        public string Comment { get; set; } = string.Empty;

        public int InterviewCriteriaItemId { get; set; }
    }
}
