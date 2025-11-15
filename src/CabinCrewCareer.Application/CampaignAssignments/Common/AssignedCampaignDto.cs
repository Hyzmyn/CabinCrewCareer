namespace CabinCrewCareer.Application.CampaignAssignments.Common
{
    public class AssignedCampaignDto
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<AssignmentDto> Assignments { get; set; } = [];
    }
}