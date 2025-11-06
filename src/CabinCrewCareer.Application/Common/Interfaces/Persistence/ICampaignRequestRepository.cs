using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface ICampaignRequestRepository : IRepository<CampaignRequest>
    {
        Task<PaginatedList<CampaignRequest>> PaginatedListAsync(
            string? searchTerm,
            string? sortColumn,
            string? sortOrder,
            int? status,
            int? requestType,
            int? partnerId,
            int page,
            int pageSize);
    }
}
