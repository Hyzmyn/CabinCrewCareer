using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class Document
    {
        public int DocumentId { get; set; }

        public string DocumentURL { get; set; } = string.Empty;

        public DocumentType Type { get; set; }

        public DateOnly? UploadDate { get; set; }

        public bool IsVerified { get; set; }

        // Foreign Keys
        public int? ApplicationId { get; set; }
        public virtual UserApplication Application { get; set; } = null!;

        // Navigation Properties
    }
}
