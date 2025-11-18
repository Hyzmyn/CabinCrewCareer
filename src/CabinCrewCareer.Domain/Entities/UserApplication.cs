using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class UserApplication
    {
        public int ApplicationId { get; set; }

        public ApplicationStatus ApplicationStatus { get; set; }

        public DateTime? SubmissionDate { get; set; }

        public string Experience { get; set; } = string.Empty;

        public int Height { get; set; } = 0;

        public int Weight { get; set; } = 0;

        public string EnglishDegreeNumber { get; set; } = string.Empty;

        public DateOnly? EndDate { get; set; }

        // Foreign Keys
        public int? UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<Document> Documents { get; set; } = [];
    }
}
