namespace CabinCrewCareer.Domain.Entities
{
    public class EnquiryRequest
    {
        public int EnquiryId { get; set; }

        public string Reason { get; set; } = string.Empty;

        public int NewScore { get; set; }

        public bool Status { get; set; }

        public DateTime CreatedAt { get; set; }

        // Foreign Keys
        public int? TestSessionId { get; set; }
        public virtual TestSession TestSession { get; set; } = null!;

        public int? UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int? ExaminerId { get; set; }
        public virtual User Examiner { get; set; } = null!;

        public virtual TestResult? TestResult { get; set; } = null!;

        // Navigation Properties
    }
}
