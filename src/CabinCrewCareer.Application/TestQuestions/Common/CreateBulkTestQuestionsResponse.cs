namespace CabinCrewCareer.Application.TestQuestions.Common
{
    public class CreateBulkTestQuestionsResponse
    {
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public int TotalQuestionsCreated { get; set; }
        public List<QuestionCreatedDto> Questions { get; set; } = [];
    }

    public class QuestionCreatedDto
    {
        public int QuestionId { get; set; }
        public string QuestionContent { get; set; } = string.Empty;
        public int Score { get; set; }
        public int OrderNumber { get; set; }
        public List<TestOptionResponse> Options { get; set; } = [];
    }
}
