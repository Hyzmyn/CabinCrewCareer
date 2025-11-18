using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.RoundActivities.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.RoundActivities.Queries
{
    public record GetRoundActivitiesQuery(
        string? SearchTerm,
        string? SortColumn,
        string? SortOrder,
        int? Status,
        int? RoundId,
        int Page,
        int PageSize) : IRequest<Response<PaginatedList<GetActivityResponse>>>;
}
