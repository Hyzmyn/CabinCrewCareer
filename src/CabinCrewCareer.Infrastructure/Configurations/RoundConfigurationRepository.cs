using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Configurations
{
    public class RoundConfigurationRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<RoundConfiguration>(dbContext, logger), IRoundConfigurationRepository
    {
        public async Task<int> PromotionInterviewBenchmark() =>
            await _dbContext.RoundConfigurations.Where(x => x.ConfigurationType == RoundConfigurationType.Interview && x.CampaignType == CampaignType.Promotion).OrderByDescending(x => x.RoundConfigurationId).Select(x => x.Benchmark).FirstOrDefaultAsync();

        public async Task<int> RecruitmentAppearanceBenchmark() =>
            await _dbContext.RoundConfigurations.Where(x => x.ConfigurationType == RoundConfigurationType.Appearance && x.CampaignType == CampaignType.Recruitment).OrderByDescending(x => x.RoundConfigurationId).Select(x => x.Benchmark).FirstOrDefaultAsync();

        public async Task<int> RecruitmentInterviewBenchmark() =>
            await _dbContext.RoundConfigurations.Where(x => x.ConfigurationType == RoundConfigurationType.Interview && x.CampaignType == CampaignType.Recruitment).OrderByDescending(x => x.RoundConfigurationId).Select(x => x.Benchmark).FirstOrDefaultAsync();
    }
}
