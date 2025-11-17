namespace CabinCrewCareer.Application.TestQuestions.Common
{
    public class CreateTestQuestionResponse
    {
        public int QuestionId { get; set; }
        public string QuestionContent { get; set; } = string.Empty;
        public int Score { get; set; }
        public int OrderNumber { get; set; }
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public List<TestOptionResponse> Options { get; set; } = [];
    }

    public class TestOptionResponse
    {
        public int OptionId { get; set; }
        public string OptionContent { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
