using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class UpdateFlightExperienceCommand : IRequest<Response<string>>
    {
        public int ActivityId { get; set; }
        public int TotalFlightHours { get; set; }
        public string? Experience { get; set; }

        public UpdateFlightExperienceCommand(int activityId, int totalFlightHours, string? experience)
        {
            ActivityId = activityId;
            TotalFlightHours = totalFlightHours;
            Experience = experience;
        }
    }
}
