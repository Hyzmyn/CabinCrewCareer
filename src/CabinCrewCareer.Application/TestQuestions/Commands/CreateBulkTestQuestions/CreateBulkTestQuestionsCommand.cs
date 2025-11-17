using CabinCrewCareer.Application.TestQuestions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestQuestions.Commands.CreateBulkTestQuestions
{
    public record CreateBulkTestQuestionsCommand(
        int TestId,
        List<QuestionDto> Questions) : IRequest<Response<CreateBulkTestQuestionsResponse>>;

    public class QuestionDto
    {
        public string QuestionContent { get; set; } = string.Empty;
        public int Score { get; set; }
        public int OrderNumber { get; set; }
        public List<TestOptionDto>? Options { get; set; }
    }
}
