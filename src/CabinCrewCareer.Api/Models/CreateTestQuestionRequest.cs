using CabinCrewCareer.Application.TestQuestions.Common;

namespace CabinCrewCareer.Api.Models
{
    public class CreateTestQuestionRequest
    {
        public int TestId { get; set; }
        public string QuestionContent { get; set; } = string.Empty;
        public int Score { get; set; }
        public int OrderNumber { get; set; }
        public List<TestOptionDto>? Options { get; set; }
    }
}
