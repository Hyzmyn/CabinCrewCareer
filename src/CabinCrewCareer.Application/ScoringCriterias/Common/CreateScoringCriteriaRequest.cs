namespace CabinCrewCareer.Application.ScoringCriterias.Common
{
    public class CreateScoringCriteriaRequest
    {
        public string ItemType { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string EnglishText { get; set; } = string.Empty;

        public List<CreateScoringCriteriaDetail> Details { get; set; } = [];
    }

    public class CreateScoringCriteriaDetail
    {
        public string DetailText { get; set; } = string.Empty;
    }
}
