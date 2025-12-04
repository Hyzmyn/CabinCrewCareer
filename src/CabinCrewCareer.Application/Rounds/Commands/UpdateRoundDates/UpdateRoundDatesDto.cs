using System;

namespace CabinCrewCareer.Application.Rounds.Commands.UpdateRoundDates
{
    public class UpdateRoundDatesDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
