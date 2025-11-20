using MediatR;

namespace CabinCrewCareer.Application.Rounds.Commands
{
    public class UpdateRoundTestIdCommand : IRequest<bool>
    {
        public int RoundId { get; set; }
        public int TestId { get; set; }

        public UpdateRoundTestIdCommand(int roundId, int testId)
        {
            RoundId = roundId;
            TestId = testId;
        }
    }
}
