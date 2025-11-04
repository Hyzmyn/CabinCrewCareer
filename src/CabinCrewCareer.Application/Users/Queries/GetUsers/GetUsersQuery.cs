using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Users.Queries.GetUsers
{
    public record GetUsersQuery(
        string? SearchTerm,
        string? SortColumn,
        string? SortOrder,
        int? IsActive,
        int? RoleId,
        int? PartnerId,
        int Page,
        int PageSize) : IRequest<Response<PaginatedList<GetUserResponse>>>;
}
