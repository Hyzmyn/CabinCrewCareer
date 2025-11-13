using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class CampaignAssignment
    {
        public int AssignmentId { get; set; }

        public TaskType Task { get; set; }

        public string TaskDescription { get; set; } = string.Empty;

        public AssignmentStatus Status { get; set; }

        public DateTime AssignedAt { get; set; }

        public string AssignedBy { get; set; } = string.Empty;

        public DateTime? CompletedAt { get; set; }

        public string? Notes { get; set; } = string.Empty;

        // Foreign Keys
        public int? UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int? CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; } = null!;
    }
}
