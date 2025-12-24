using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface ICampaignRepository : IRepository<Campaign>
    {
        Task<PaginatedList<Campaign>> PaginatedListAsync
            (string? searchTerm
            , string? sortColumn
            , string? sortOrder
            , int? campaignType
            , int? campaignStatus
            , int? partnerId
            , int page
            , int pageSize);
    }
}
