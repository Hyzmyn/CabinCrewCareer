using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestSessions.Commands.ScoreSpeakingTestSession
{
    public class ScoreSpeakingTestSessionCommand : IRequest<Response<string>>
    {
        public int TestSessionId { get; set; }
        public Dictionary<string, int> CriteriaScores { get; set; } = new();
    }
}
