namespace CabinCrewCareer.Application.Roles.Common
{
    public class GetRoleResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public List<AssignedCampaignSimpleDto> AssignedCampaigns { get; set; } = new();
        public int Count => AssignedCampaigns?.Count ?? 0;
    }

    public class AssignedCampaignSimpleDto
    {
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}
