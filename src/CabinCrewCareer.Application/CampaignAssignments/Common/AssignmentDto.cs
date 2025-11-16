namespace CabinCrewCareer.Application.CampaignAssignments.Common
{
    public class AssignmentDto
    {
        public int AssignmentId { get; set; }
        public string Task { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
    }
}