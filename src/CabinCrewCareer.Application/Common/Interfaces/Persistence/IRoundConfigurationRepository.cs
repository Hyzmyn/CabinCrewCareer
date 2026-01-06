using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface IRoundConfigurationRepository : IRepository<RoundConfiguration>
    {
        Task<int> RecruitmentAppearanceBenchmark();

        Task<int> RecruitmentInterviewBenchmark();

        Task<int> PromotionInterviewBenchmark();
    }
}
