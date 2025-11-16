using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Infrastructure.Services
{
    /// <summary>
    /// Service tự động cập nhật trạng thái assignments dựa trên trạng thái campaign
    /// </summary>
    public class AssignmentStatusService : IAssignmentStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public AssignmentStatusService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task UpdateAssignmentStatusByCampaignStatusAsync(int campaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(campaignId);
            if (campaign == null) return;

            var assignments = await _unitOfWork.CampaignAssignments.GetAssignmentsByCampaignIdAsync(campaignId);
            if (!assignments.Any()) return;

            bool hasChanges = false;

            foreach (var assignment in assignments)
            {
                // Bỏ qua assignments đã bị cancelled
                if (assignment.Status == AssignmentStatus.Cancelled) continue;

                var newStatus = DetermineAssignmentStatus(assignment.Status, campaign.CampaignStatus);

                if (newStatus != assignment.Status)
                {
                    assignment.Status = newStatus;

                    // Cập nhật CompletedAt nếu chuyển sang Completed
                    if (newStatus == AssignmentStatus.Completed && !assignment.CompletedAt.HasValue)
                    {
                        assignment.CompletedAt = _dateTimeProvider.UtcNow;
                    }

                    await _unitOfWork.CampaignAssignments.UpdateAsync(assignment);
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task UpdateAllAssignmentStatusesAsync(CancellationToken cancellationToken = default)
        {
            var campaigns = await _unitOfWork.Campaigns.GetAllAsync();
            var activeCampaigns = campaigns.Where(c => c.CampaignStatus >= CampaignStatus.Published).ToList();

            if (!activeCampaigns.Any()) return;

            foreach (var campaign in activeCampaigns)
            {
                await UpdateAssignmentStatusByCampaignStatusAsync(campaign.CampaignId, cancellationToken);
            }
        }

        /// <summary>
        /// Xác định assignment status mới dựa trên assignment status hiện tại và campaign status
        /// </summary>
        private static AssignmentStatus DetermineAssignmentStatus(AssignmentStatus currentAssignmentStatus, CampaignStatus campaignStatus)
        {
            // Logic: 
            // - Published -> Assigned giữ nguyên (đã được giao nhưng chưa bắt đầu)
            // - Ongoing -> InProgress tự động (chiến dịch đang diễn ra)
            // - Ended -> Completed tự động (chiến dịch kết thúc)

            return campaignStatus switch
            {
                // Campaign Published: giữ nguyên Assigned
                CampaignStatus.Published => currentAssignmentStatus == AssignmentStatus.Assigned 
                    ? AssignmentStatus.Assigned 
                    : currentAssignmentStatus,

                // Campaign Upcoming: giữ nguyên Assigned
                CampaignStatus.Upcoming => currentAssignmentStatus == AssignmentStatus.Assigned 
                    ? AssignmentStatus.Assigned 
                    : currentAssignmentStatus,

                // Campaign Ongoing: Assigned -> InProgress tự động
                CampaignStatus.Ongoing => currentAssignmentStatus == AssignmentStatus.Assigned 
                    ? AssignmentStatus.InProgress 
                    : currentAssignmentStatus,

                // Campaign Ended: InProgress -> Completed tự động
                CampaignStatus.Ended => currentAssignmentStatus == AssignmentStatus.InProgress 
                    ? AssignmentStatus.Completed 
                    : currentAssignmentStatus,

                // Các trạng thái khác: giữ nguyên
                _ => currentAssignmentStatus
            };
        }
    }
}
