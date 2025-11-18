namespace CabinCrewCareer.Application.ScoringCriterias.Common
{
    public class GetScoringCriteriaResponse
    {
        public string Title { get; set; } = string.Empty;

        public List<GetScoringCriteriaItemResponse> Items { get; set; } = [];
    }

    public class GetScoringCriteriaItemResponse
    {
        public int ScoringCriteriaItemId { get; set; }

        public string ItemType { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string EnglishText { get; set; } = string.Empty;

        public List<GetScoringCriteriaDetailResponse> Details { get; set; } = [];
    }

    public class GetScoringCriteriaDetailResponse
    {
        public string DetailText { get; set; } = string.Empty;
    }
}
