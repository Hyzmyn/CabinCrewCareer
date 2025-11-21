namespace CabinCrewCareer.Application.Tests.Common
{
    public class GetAvailableTestResponse
    {
        public int TestId { get; set; }

        public string TestName { get; set; } = string.Empty;

        public string TestType { get; set; } = string.Empty;

        public int NumberOfQuestions { get; set; } = 0;
    }
}
