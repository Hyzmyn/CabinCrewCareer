using CabinCrewCareer.Application.Roles.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Roles.Queries.GetUsersByRoleId
{
    public record GetUsersByRoleIdQuery(int? RoleId) : IRequest<Response<List<GetRoleResponse>>>;
}
