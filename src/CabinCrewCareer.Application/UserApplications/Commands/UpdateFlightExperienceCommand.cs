using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class UpdateFlightExperienceCommand : IRequest<Response<string>>
    {
        public int ApplicationSnapshotId { get; set; }
        public int TotalFlightHours { get; set; }
        public string? Experience { get; set; }

        public UpdateFlightExperienceCommand(int applicationSnapshotId, int totalFlightHours, string? experience)
        {
            ApplicationSnapshotId = applicationSnapshotId;
            TotalFlightHours = totalFlightHours;
            Experience = experience;
        }
    }
}
