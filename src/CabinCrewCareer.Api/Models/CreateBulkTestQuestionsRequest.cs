using CabinCrewCareer.Application.TestQuestions.Commands.CreateBulkTestQuestions;

namespace CabinCrewCareer.Api.Models
{
    public class CreateBulkTestQuestionsRequest
    {
        public int TestId { get; set; }
        public List<QuestionDto> Questions { get; set; } = [];
    }
}
