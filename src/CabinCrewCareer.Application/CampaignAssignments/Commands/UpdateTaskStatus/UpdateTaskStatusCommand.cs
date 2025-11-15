using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignAssignments.Commands.UpdateTaskStatus
{
    public class UpdateTaskStatusCommand : IRequest<Response<bool>>
    {
        public int AssignmentId { get; set; } // From URL parameter
        public AssignmentStatus Status { get; set; } // Request body only contains Status
    }
}