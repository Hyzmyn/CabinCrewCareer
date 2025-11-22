namespace CabinCrewCareer.Application.Tests.Common
{
    public class GetTestsByUserIdResponse
    {
        public int UserId { get; set; }
        public int TotalTests { get; set; }
        public List<UserTestDto> Tests { get; set; } = [];
    }

    public class UserTestDto
    {
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public int DurationInMinutes { get; set; }
        public string JoinCode { get; set; } = string.Empty;
        public int RoundId { get; set; }
        public string RoundType { get; set; } = string.Empty;
        public DateTime? RoundStartDate { get; set; }
        public DateTime? RoundEndDate { get; set; }
    }
}
