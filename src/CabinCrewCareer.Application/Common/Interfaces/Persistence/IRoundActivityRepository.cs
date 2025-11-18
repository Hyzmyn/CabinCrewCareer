using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface IRoundActivityRepository : IRepository<RoundActivity>
    {
        Task<PaginatedList<RoundActivity>> PaginatedListAsync
            (string? searchTerm
            , string? sortColumn
            , string? sortOrder
            , int? status
            , int? roundId
            , int page
            , int pageSize);

        Task<List<RoundActivity>> GetRoundParticipantsAsync(int roundId);
    }
}
