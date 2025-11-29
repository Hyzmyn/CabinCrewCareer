namespace CabinCrewCareer.Application.TestSessions.Common
{
    public class TestSessionWithStatusResponse
    {
        public int TestSessionId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? ImgURL { get; set; } = string.Empty;
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalScore { get; set; }
        public int MaxScore { get; set; }
        public bool Status { get; set; } // Finalized or not
        public DateTime CreatedAt { get; set; }
        public int TotalAnswers { get; set; }

        // New fields for RoundActivity status
        public string ActivityStatus { get; set; } = string.Empty; // "Ongoing", "Passed", "Failed"
        public int? RoundId { get; set; }
        public string? RoundType { get; set; }
        public int? ActivityId { get; set; }
    }
}
