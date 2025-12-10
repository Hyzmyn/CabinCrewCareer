namespace CabinCrewCareer.Application.Requirements.Common
{
    public class GetRequirementResponse
    {
        public int RequirementId { get; set; }

        public List<GetRequirementItemResponse> RequirementItems { get; set; } = [];
    }

    public class GetRequirementItemResponse
    {
        public int RequirementItemId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
