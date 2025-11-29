using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetTestSessionsByRound
{
    /// <summary>
    /// Get all test sessions by RoundId with their RoundActivity status
    /// </summary>
    public class GetTestSessionsByRoundQuery : IRequest<Response<PaginatedList<TestSessionWithStatusResponse>>>
    {
        public int RoundId { get; set; }
        public TestType? TestType { get; set; } // Filter by test type
        public string? SearchTerm { get; set; } // Search by user name or email
        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
