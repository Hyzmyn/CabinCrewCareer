using CabinCrewCareer.Application.TestQuestions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestQuestions.Commands.CreateTestQuestion
{
    public record CreateTestQuestionCommand(
        int TestId,
        string QuestionContent,
        int Score,
        int OrderNumber,
        List<TestOptionDto>? Options = null) : IRequest<Response<CreateTestQuestionResponse>>;
}
