namespace CabinCrewCareer.Application.EvaluationForms.Common
{
    public class GetAppearanceEvaluationFormResponse
    {
        public int EvaluationId { get; set; }

        public string EvaluatedDate { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        public bool IsPassed { get; set; } = false;

        public string RoundName { get; set; } = string.Empty;

        public string Candidate { get; set; } = string.Empty;

        public string Examiner { get; set; } = string.Empty;

        public string Benchmark { get; set; } = string.Empty;

        public List<GetAppearanceResultResponse> AppearanceResults { get; set; } = [];
    }

    public class GetAppearanceResultResponse
    {
        public int ScoringCriteriaItemId { get; set; }

        public bool IsPassed { get; set; }
    }
}
