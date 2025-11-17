using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CabinCrewCareer.Infrastructure.Users
{
    public class UserRepository(
        CabinCrewCareerDbContext dbContext,
        ILogger logger) : Repository<User>(dbContext, logger), IUserRepository
    {
        public async Task<PaginatedList<User>> PaginatedListAsync(string? searchTerm, string? sortColumn, string? sortOrder, int? isActive, int? roleId, int? partnerId, int page, int pageSize)
        {
            IQueryable<User> usersQuery = _dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.AirlinePartner);

            switch (isActive)
            {
                case 1:
                    usersQuery = usersQuery.Where(u => u.IsActive);
                    break;
                case 0:
                    usersQuery = usersQuery.Where(u => !u.IsActive);
                    break;
                default: break;
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                usersQuery = usersQuery.Where(c =>
                    c.FullName.Contains(searchTerm));
            }

            if (roleId != null)
            {
                usersQuery = usersQuery.Where(c => c.RoleId == roleId);
            }

            if (partnerId != null)
            {
                usersQuery = usersQuery.Where(c => c.PartnerId == partnerId);
            }

            if (sortOrder?.ToLower() == "asc")
            {
                usersQuery = usersQuery.OrderBy(GetSortProperty(sortColumn));
            }
            else
            {
                usersQuery = usersQuery.OrderByDescending(GetSortProperty(sortColumn));
            }

            var users = await PaginatedList<User>.CreateAsync(usersQuery, page, pageSize);

            return users;
        }

        private static Expression<Func<User, object>> GetSortProperty(string? sortColumn)
       => sortColumn?.ToLower() switch
       {
           "name" => user => user.FullName,
           //"dob" => user => user.DoB,
           _ => user => user.UserId
       };

        public async Task<List<User>> GetUsersByRoleIdAsync(int? roleId)
        {
            return await _dbContext.Users
                .Where(u => u.RoleId == roleId)
                .Select(u => new User
                {
                    UserId = u.UserId,
                    FullName = u.FullName
                }).ToListAsync();
        }

        public async Task<bool> CheckExistEmailAsync(string email)
        {
            var user = await _dbContext.Users.Where(u => u.Email.Equals(email)).Select(u => new User { UserId = u.UserId }).FirstOrDefaultAsync();

            if (user is null) return false;

            return true;
        }

        public async Task<bool> CheckExistPhoneNumberAsync(string phoneNumber)
        {
            var user = await _dbContext.Users.Where(u => u.PhoneNumber.Equals(phoneNumber)).Select(u => new User { UserId = u.UserId }).FirstOrDefaultAsync();

            if (user is null) return false;

            return true;
        }
    }
}
