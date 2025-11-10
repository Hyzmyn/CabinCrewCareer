namespace CabinCrewCareer.Application.Tests.Common
{
    public class GetTestResponse
    {
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public string? AudioFileURL { get; set; }
        public string JoinCode { get; set; } = string.Empty;
        public string? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
