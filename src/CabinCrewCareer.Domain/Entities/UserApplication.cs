using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class UserApplication
    {
        public int ApplicationId { get; set; }

        public ApplicationStatus ApplicationStatus { get; set; }

        public DateTime? SubmissionDate { get; set; }

        // Foreign Keys
        public int? UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int? RoundId { get; set; }
        public virtual Round Round { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<Document> Documents { get; set; } = [];
    }
}
