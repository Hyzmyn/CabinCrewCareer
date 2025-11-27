using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetMyTestSessionsByType
{
    public class GetMyTestSessionsByTypeQuery : IRequest<Response<PaginatedList<GetTestSessionResponse>>>
    {
        public int TestType { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; }
        public int? UserId { get; set; }
        public int? RoundId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
