namespace CabinCrewCareer.Domain.Entities
{
    public class TestResult
    {
        public int ResultId { get; set; }

        public int Score { get; set; }

        public DateOnly TestDate { get; set; }

        public int FinalScore { get; set; }

        public bool EnquiryRequested { get; set; } = false;

        public string EnquiryBy { get; set; } = string.Empty;

        public string EnquiryNote { get; set; } = string.Empty;

        public string ExaminedBy { get; set; } = string.Empty;

        public DateTime ExaminedAt { get; set; }

        public bool Status { get; set; }

        // Foreign Keys
        //public int? TestId { get; set; }
        //public virtual Test Test { get; set; } = null!;

        public int? UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int? EnquiryId { get; set; }
        public virtual EnquiryRequest EnquiryRequest { get; set; } = null!;

        public int? TestSessionId { get; set; }
        public virtual TestSession TestSession { get; set; } = null!;

        // Navigation Properties
    }
}
