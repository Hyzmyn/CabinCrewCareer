using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Users.Queries.GetUserHistory
{
    public record GetUserHistoryQuery : IRequest<Response<List<GetUserCampaignResponse>>>;
}
