using MediatR;

namespace CabinCrewCareer.Application.Rounds.Queries
{
    public class GetRoundTestInfoQuery : IRequest<RoundTestInfoResponse?>
    {
        public int RoundId { get; set; }
        public GetRoundTestInfoQuery(int roundId)
        {
            RoundId = roundId;
        }
    }
}
