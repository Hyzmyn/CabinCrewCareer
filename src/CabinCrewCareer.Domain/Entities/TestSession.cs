namespace CabinCrewCareer.Domain.Entities
{
    public class TestSession
    {
        public int TestSessionId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool Status { get; set; }

        public int TotalScore { get; set; }

        public DateTime CreatedAt { get; set; }

        // Foreign Keys
        public int? UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int? TestId { get; set; }
        public virtual Test Test { get; set; } = null!;

        public virtual EnquiryRequest? EnquiryRequest { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<TestAnswer> TestAnswers { get; set; } = [];
        public virtual ICollection<TestResult> TestResults { get; set; } = [];
    }
}
