using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Users
{
    public class UserRepository(
        CabinCrewCareerDbContext dbContext,
        ILogger logger) : Repository<User>(dbContext, logger), IUserRepository
    {
        public Task<PaginatedList<User>> PaginatedListAsync(string? searchTerm, string? sortColumn, string? sortOrder, int? roleId, int? gradeId, int? schooId, int? isActive, int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
