namespace CabinCrewCareer.Application.InterviewCriterias.Common
{
    public class GetInterviewCriteriaResponse
    {
        public string Title { get; set; } = string.Empty;

        public List<GetInterviewCriteriaItemResponse> Items { get; set; } = [];
    }

    public class GetInterviewCriteriaItemResponse
    {
        public int InterviewCriteriaItemId { get; set; }

        public string Criteria { get; set; } = string.Empty;
    }
}
