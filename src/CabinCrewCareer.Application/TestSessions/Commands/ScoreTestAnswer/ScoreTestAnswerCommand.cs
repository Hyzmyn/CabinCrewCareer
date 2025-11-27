using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestSessions.Commands.ScoreTestAnswer
{
    public class ScoreTestAnswerCommand : IRequest<Response<string>>
    {
        public int TestSessionId { get; set; }
        public Dictionary<int, AnswerScoreDto> AnswerScores { get; set; } = new();
    }

    public class AnswerScoreDto
    {
        public Dictionary<string, int> CriteriaScores { get; set; } = new();
        public bool IsCorrect { get; set; }
    }
}
