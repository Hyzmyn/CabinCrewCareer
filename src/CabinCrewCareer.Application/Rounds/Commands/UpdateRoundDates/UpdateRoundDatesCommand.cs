using MediatR;
using System;

namespace CabinCrewCareer.Application.Rounds.Commands.UpdateRoundDates
{
    public class UpdateRoundDatesCommand : IRequest<bool>
    {
        public int RoundId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
