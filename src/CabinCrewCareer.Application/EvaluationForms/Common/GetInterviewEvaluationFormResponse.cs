namespace CabinCrewCareer.Application.EvaluationForms.Common
{
    public class GetInterviewEvaluationFormResponse
    {
        public int EvaluationId { get; set; }

        public string EvaluatedDate { get; set; } = string.Empty;

        public bool IsPassed { get; set; } = false;

        public string RoundName { get; set; } = string.Empty;

        public string Candidate { get; set; } = string.Empty;

        public string Examiner { get; set; } = string.Empty;

        public int FinalScore { get; set; } = 0;
    }
}
