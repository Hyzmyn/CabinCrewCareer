using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetMyEnglishTestSessions
{
    /// <summary>
    /// Get my English test sessions (Listening + Speaking) for candidates with RoundActivity status
    /// </summary>
    public class GetMyEnglishTestSessionsQuery : IRequest<Response<PaginatedList<TestSessionWithStatusResponse>>>
    {
        public int? RoundId { get; set; } // Optional filter by round
        public string? SearchTerm { get; set; }
        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
