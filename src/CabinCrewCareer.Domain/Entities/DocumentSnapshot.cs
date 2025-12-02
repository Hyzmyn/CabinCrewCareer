using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class DocumentSnapshot
    {
        public int DocumentSnapshotId { get; set; }

        public string DocumentURL { get; set; } = string.Empty;

        public DocumentType Type { get; set; }

        public DateOnly? UploadDate { get; set; }

        public bool IsVerified { get; set; }

        // Foreign Keys
        public int? ApplicationSnapshotId { get; set; }
        public virtual UserApplicationSnapshot UserApplicationSnapshot { get; set; } = null!;

        // Navigation Properties
    }
}
