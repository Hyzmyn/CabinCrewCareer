using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface IRoundConfigurationRepository : IRepository<RoundConfiguration>
    {
        Task<int> RecruitmentAppearanceBenchmark();

        Task<int> RecruitmentInterviewBenchmark();

        Task<int> PromotionInterviewBenchmark();

        Task<int> EnglishListeningTestBenchmark();

        Task<int> EnglishSpeakingTestBenchmark();

        Task<int> PracticalTestBenchmark();

        Task<RoundConfiguration?> GetCurrentlyEffectiveConfiguration(int roundTypeId);

        Task<PaginatedList<RoundConfiguration>> PaginatedListAsync
            (string? sortColumn
            , string? sortOrder
            , int? campaignType
            , int? roundTypeId
            , int page
            , int pageSize);
    }
}
