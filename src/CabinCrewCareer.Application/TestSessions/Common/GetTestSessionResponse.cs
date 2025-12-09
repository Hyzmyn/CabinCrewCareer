namespace CabinCrewCareer.Application.TestSessions.Common
{
    public class GetTestSessionResponse
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
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalAnswers { get; set; }
        
        /// <summary>
        /// Indicates if the user has been evaluated in RoundActivity.
        /// true = Has been evaluated (Passed or Failed)
        /// false = Not yet evaluated (Still Ongoing)
        /// </summary>
        public bool IsPassedOrFailed { get; set; }
    }
}
