using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CabinCrewCareer.Infrastructure.Configurations
{
    public class RoundConfigurationRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<RoundConfiguration>(dbContext, logger), IRoundConfigurationRepository
    {
        public async Task<PaginatedList<RoundConfiguration>> PaginatedListAsync(string? sortColumn, string? sortOrder, int? campaignType, int? roundTypeId, int page, int pageSize)
        {
            IQueryable<RoundConfiguration> usersQuery = _dbContext.RoundConfigurations
                .Include(u => u.RoundType);

            if (campaignType != null)
            {
                usersQuery = usersQuery.Where(x => x.RoundType.CampaignType == (CampaignType)campaignType);
            }

            if (roundTypeId != null)
            {
                usersQuery = usersQuery.Where(x => x.RoundTypeId == roundTypeId);
            }

            if (sortOrder?.ToLower() == "asc")
            {
                usersQuery = usersQuery.OrderBy(GetSortProperty(sortColumn));
            }
            else
            {
                usersQuery = usersQuery.OrderByDescending(GetSortProperty(sortColumn));
            }

            var users = await PaginatedList<RoundConfiguration>.CreateAsync(usersQuery, page, pageSize);

            return users;
        }

        private static Expression<Func<RoundConfiguration, object>> GetSortProperty(string? sortColumn)
       => sortColumn?.ToLower() switch
       {
           "date" => user => user.EffectiveDate,
           //"dob" => user => user.DoB,
           _ => user => user.RoundConfigurationId
       };

        public async Task<int> PromotionInterviewBenchmark() =>
            await _dbContext.RoundConfigurations.Where(x => x.RoundTypeId == (int?)RoundConfigurationType.PromotionInterview).OrderByDescending(x => x.RoundConfigurationId).Select(x => x.Benchmark).FirstOrDefaultAsync();

        public async Task<int> RecruitmentAppearanceBenchmark() =>
            await _dbContext.RoundConfigurations.Where(x => x.RoundTypeId == (int?)RoundConfigurationType.Appearance).OrderByDescending(x => x.RoundConfigurationId).Select(x => x.Benchmark).FirstOrDefaultAsync();

        public async Task<int> RecruitmentInterviewBenchmark() =>
            await _dbContext.RoundConfigurations.Where(x => x.RoundTypeId == (int?)RoundConfigurationType.RecruitmentInterview).OrderByDescending(x => x.RoundConfigurationId).Select(x => x.Benchmark).FirstOrDefaultAsync();

        public async Task<int> EnglishListeningTestBenchmark() =>
            await _dbContext.RoundConfigurations.Where(x => x.RoundTypeId == (int?)RoundConfigurationType.EnglishListeningTest).OrderByDescending(x => x.RoundConfigurationId).Select(x => x.Benchmark).FirstOrDefaultAsync();

        public async Task<int> EnglishSpeakingTestBenchmark() =>
            await _dbContext.RoundConfigurations.Where(x => x.RoundTypeId == (int?)RoundConfigurationType.EnglishSpeakingTest).OrderByDescending(x => x.RoundConfigurationId).Select(x => x.Benchmark).FirstOrDefaultAsync();

        public async Task<int> PracticalTestBenchmark() =>
            await _dbContext.RoundConfigurations.Where(x => x.RoundTypeId == (int?)RoundConfigurationType.PracticalTest).OrderByDescending(x => x.RoundConfigurationId).Select(x => x.Benchmark).FirstOrDefaultAsync();

        public async Task<RoundConfiguration?> GetCurrentlyEffectiveConfiguration(int roundTypeId)
            => await _dbContext.RoundConfigurations.Where(x => x.RoundTypeId == roundTypeId && x.Status).FirstOrDefaultAsync();
    }
}
