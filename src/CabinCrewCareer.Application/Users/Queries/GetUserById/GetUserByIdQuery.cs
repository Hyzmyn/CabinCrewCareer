using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Users.Queries.GetUserById
{
    public record GetUserByIdQuery(int UserId) : IRequest<Response<GetUserDetailResponse>>;
}
