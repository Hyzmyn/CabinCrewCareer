using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.RoundActivities.Commands.MoveToInterviewRound
{
    public class MoveToInterviewRoundCommand : IRequest<Response<string>>
    {
        public int RoundId { get; set; }
    }
}
