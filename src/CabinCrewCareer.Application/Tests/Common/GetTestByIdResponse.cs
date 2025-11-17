using System.Text.Json.Serialization;

namespace CabinCrewCareer.Application.Tests.Common
{
    public class GetTestByIdResponse
    {
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public int DurationInMinutes { get; set; }
        public int TotalQuestions { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AudioFileURL { get; set; }
        
        public string JoinCode { get; set; } = string.Empty;
        public string? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
