using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface IUserRepository : IRepository<User>
    {
        Task<PaginatedList<User>> PaginatedListAsync
            (string? searchTerm
            , string? sortColumn
            , string? sortOrder
            , int? isActive
            , int? roleId
            , int? partnerId
            , int page
            , int pageSize);

        Task<List<User>> GetUsersByRoleIdAsync(int? roleId);
    }
}
