using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestSessions.Commands.SubmitMultipleChoiceTest
{
    public record SubmitMultipleChoiceTestCommand(
        int TestId,
        DateTime StartTime,
        DateTime EndTime,
        List<MultipleChoiceAnswerDto> Answers) : IRequest<Response<SubmitTestSessionResponse>>;
}
