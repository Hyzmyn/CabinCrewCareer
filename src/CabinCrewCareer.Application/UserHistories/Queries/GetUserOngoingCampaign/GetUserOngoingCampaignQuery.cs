using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.UserHistories.Queries.GetUserOngoingCampaign
{
    public record GetUserOngoingCampaignQuery : IRequest<Response<GetUserCampaignResponse>>;
}
