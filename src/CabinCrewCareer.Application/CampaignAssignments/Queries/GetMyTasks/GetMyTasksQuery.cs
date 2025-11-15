using CabinCrewCareer.Application.CampaignAssignments.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignAssignments.Queries.GetMyTasks
{
    public class GetMyTasksQuery : IRequest<Response<List<AssignedCampaignDto>>>
    {
    }
}