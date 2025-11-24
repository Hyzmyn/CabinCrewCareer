using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetAllTestSessions
{
    public class GetAllTestSessionsQuery : IRequest<Response<PaginatedList<GetTestSessionResponse>>>
    {
        public string? SearchTerm { get; set; }
        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; }
        public int? TestId { get; set; }
        public int? UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
