using CabinCrewCareer.Application.UserApplications.Common;

namespace CabinCrewCareer.Application.UserApplications.Queries.GetUserApplicationByUserId
{
    public record GetUserApplicationByUserIdQuery(int UserId) : MediatR.IRequest<Response<UserApplicationDetailResponse>>;
}
