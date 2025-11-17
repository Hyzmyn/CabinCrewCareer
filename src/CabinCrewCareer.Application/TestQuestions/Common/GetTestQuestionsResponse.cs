namespace CabinCrewCareer.Application.TestQuestions.Common
{
    public class GetTestQuestionsResponse
    {
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public List<TestQuestionDto> Questions { get; set; } = [];
    }

    public class TestQuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionContent { get; set; } = string.Empty;
        public int Score { get; set; }
        public int OrderNumber { get; set; }
        public List<TestOptionResponse>? Options { get; set; } = [];
    }
}
