using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Application.Tests.Commands.CreateTest
{
    public class CreateTestDto
    {
        public string TestName { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public TestType TestType { get; set; }
        public int MaxScore { get; set; }
        public string? AudioFileURL { get; set; }
    }
}
