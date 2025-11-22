using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestSessions.Commands.SubmitTestSession
{
    public record SubmitTestSessionCommand(
        int TestId,
        DateTime StartTime,
        DateTime EndTime,
        List<TestAnswerDto> Answers) : IRequest<Response<SubmitTestSessionResponse>>;
}
