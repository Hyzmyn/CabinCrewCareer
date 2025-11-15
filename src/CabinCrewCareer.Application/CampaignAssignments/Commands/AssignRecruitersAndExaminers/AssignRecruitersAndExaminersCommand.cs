using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignAssignments.Commands.AssignRecruitersAndExaminers
{
    public class UserTaskAssignment
    {
        public int UserId { get; set; }
        public TaskType TaskType { get; set; }
        public string? TaskDescription { get; set; }
    }

    public class AssignRecruitersAndExaminersCommand : IRequest<Response<bool>>
    {
        public int CampaignId { get; set; }
        public List<UserTaskAssignment> Assignments { get; set; } = [];
    }
}