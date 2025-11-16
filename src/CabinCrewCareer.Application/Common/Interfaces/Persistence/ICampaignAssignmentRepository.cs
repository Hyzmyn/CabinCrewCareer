using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface ICampaignAssignmentRepository : IRepository<CampaignAssignment>
    {
        Task<List<CampaignAssignment>> GetAssignmentsByUserIdAsync(int userId);
        Task<List<CampaignAssignment>> GetAssignmentsByCampaignIdAsync(int campaignId);
        Task<bool> HasAssignmentsForCampaignAsync(int campaignId);
        Task<int> GetOngoingCampaignsCountByUserIdAsync(int userId);
        Task<bool> HasOverlappingCampaignAsync(int userId, int campaignId);
    }
}