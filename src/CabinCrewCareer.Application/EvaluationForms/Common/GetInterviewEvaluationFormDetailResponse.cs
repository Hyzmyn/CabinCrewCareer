namespace CabinCrewCareer.Application.EvaluationForms.Common
{
    public class GetInterviewEvaluationFormDetailResponse
    {
        public int EvaluationId { get; set; }

        public string EvaluatedDate { get; set; } = string.Empty;

        public bool IsPassed { get; set; } = false;

        public string RoundName { get; set; } = string.Empty;

        public string Candidate { get; set; } = string.Empty;

        public string Examiner { get; set; } = string.Empty;

        public int FinalScore { get; set; } = 0;

        public List<GetInterviewResultResponse> InterviewResults { get; set; } = [];
    }

    public class GetInterviewResultResponse
    {
        public int InterviewCriteriaItemId { get; set; }

        public int Score { get; set; } = 0;

        public string Comment { get; set; } = string.Empty;
    }
}
