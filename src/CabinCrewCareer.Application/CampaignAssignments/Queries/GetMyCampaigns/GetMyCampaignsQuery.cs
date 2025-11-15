using CabinCrewCareer.Application.CampaignAssignments.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignAssignments.Queries.GetMyCampaigns
{
    public class GetMyCampaignsQuery : IRequest<Response<List<SimpleCampaignDto>>>
    {
    }
}