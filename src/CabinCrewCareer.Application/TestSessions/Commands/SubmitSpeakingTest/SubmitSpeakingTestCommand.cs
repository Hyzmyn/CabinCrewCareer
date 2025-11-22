using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CabinCrewCareer.Application.TestSessions.Commands.SubmitSpeakingTest
{
    public record SubmitSpeakingTestCommand(
        int TestId,
        DateTime StartTime,
        DateTime EndTime,
        List<SpeakingAnswerDto> Answers) : IRequest<Response<SubmitTestSessionResponse>>;

    public class SpeakingAnswerDto
    {
        public int QuestionId { get; set; }
        public IFormFile AudioFile { get; set; } = null!;
    }
}
