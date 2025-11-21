namespace CabinCrewCareer.Application.Rounds.Queries
{
    public class RoundTestInfoResponse
    {
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public string AudioFileURL { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
    }
}
