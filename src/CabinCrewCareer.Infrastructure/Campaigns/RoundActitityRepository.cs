using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    public class RoundActitityRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<RoundActivity>(dbContext, logger), IRoundActivityRepository
    {
        public async Task<List<RoundActivity>> GetRoundParticipantsAsync(int roundId)
        {
            var activities = await _dbContext.RoundActivities.Select(u => new RoundActivity
            {
                ActivityId = u.ActivityId,
                Status = u.Status,
                Round = new Round
                {
                    RoundId = u.Round.RoundId,
                    RoundType = u.Round.RoundType
                },
                User = new User
                {
                    UserId = u.User.UserId,
                    ImgURL = u.User.ImgURL,
                    FullName = u.User.FullName
                }
            }).Where(u => u.RoundId == roundId).ToListAsync();

            return activities;
        }

        private static Expression<Func<RoundActivity, object>> GetSortProperty(string? sortColumn)
       => sortColumn?.ToLower() switch
       {
           "name" => activity => activity.User.FullName,
           //"dob" => user => user.DoB,
           _ => activity => activity.ActivityId
       };

        public async Task<PaginatedList<RoundActivity>> PaginatedListAsync(string? searchTerm, string? sortColumn, string? sortOrder, int? status, int? roundId, int page, int pageSize)
        {
            var activities = _dbContext.RoundActivities.Select(u => new RoundActivity
            {
                ActivityId = u.ActivityId,
                Status = u.Status,
                RoundId = u.RoundId,
                UserId = u.UserId,
                ApplicationSnapshotId = u.ApplicationSnapshotId,
                Round = new Round
                {
                    RoundType = u.Round.RoundType
                },
                User = new User
                {
                    ImgURL = u.User.ImgURL,
                    FullName = u.User.FullName,
                    Email = u.User.Email,
                    PhoneNumber = u.User.PhoneNumber
                }
            });

            switch (status)
            {
                case 1:
                    activities = activities.Where(u => u.Status == ActivityStatus.Ongoing);
                    break;
                case 2:
                    activities = activities.Where(u => u.Status == ActivityStatus.Passed);
                    break;
                case 3:
                    activities = activities.Where(u => u.Status == ActivityStatus.Failed);
                    break;
                default: break;
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                activities = activities.Where(c =>
                    c.User.FullName.Contains(searchTerm));
            }

            if (roundId != null)
            {
                activities = activities.Where(c => c.RoundId == roundId);
            }

            if (sortOrder?.ToLower() == "asc")
            {
                activities = activities.OrderBy(GetSortProperty(sortColumn));
            }
            else
            {
                activities = activities.OrderByDescending(GetSortProperty(sortColumn));
            }

            var users = await PaginatedList<RoundActivity>.CreateAsync(activities, page, pageSize);

            return users;
        }

        public async Task<RoundActivity?> GetRoundActivityAsync(int roundId, int userId)
        {
            var result = await _dbContext.RoundActivities
                .Where(x => x.RoundId == roundId && x.UserId == userId)
                .Select(x => new RoundActivity
                {
                    ActivityId = x.ActivityId,
                    Status = x.Status,
                    ApplicationSnapshotId = x.ApplicationSnapshotId
                }).FirstOrDefaultAsync();

            if (result is null) return null;

            return result;
        }
    }
}
