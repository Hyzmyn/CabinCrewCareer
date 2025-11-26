using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.UserHistories.Queries.GetUserCompletedCampaigns
{
    public record GetUserCompletedCampaignsQuery : IRequest<Response<List<GetUserCampaignResponse>>>;
}
