using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Infrastructure.Services
{
    public class CampaignStatusService : ICampaignStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CampaignStatusService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task UpdateCampaignStatusByDateAsync(int campaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(campaignId);
            if (campaign == null) return;

            // Chỉ cập nhật status cho campaigns ở trạng thái Published trở lên
            if (campaign.CampaignStatus < CampaignStatus.Published) return;

            var currentTime = _dateTimeProvider.UtcNow;
            var newStatus = DetermineStatusByDate(campaign.StartDate, campaign.EndDate, currentTime);

            // Chỉ cập nhật nếu status thực sự thay đổi
            if (newStatus != campaign.CampaignStatus)
            {
                campaign.CampaignStatus = newStatus;
                await _unitOfWork.Campaigns.UpdateAsync(campaign);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task UpdateAllCampaignStatusesAsync(CancellationToken cancellationToken = default)
        {
            // Lấy tất cả campaigns có status >= Published
            var campaigns = await _unitOfWork.Campaigns.GetAllAsync();
            var campaignsToUpdate = campaigns.Where(c => c.CampaignStatus >= CampaignStatus.Published).ToList();

            if (!campaignsToUpdate.Any()) return;

            var currentTime = _dateTimeProvider.UtcNow;
            bool hasChanges = false;

            foreach (var campaign in campaignsToUpdate)
            {
                var newStatus = DetermineStatusByDate(campaign.StartDate, campaign.EndDate, currentTime);
                
                if (newStatus != campaign.CampaignStatus)
                {
                    campaign.CampaignStatus = newStatus;
                    await _unitOfWork.Campaigns.UpdateAsync(campaign);
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _unitOfWork.CompleteAsync();
            }
        }

        /// <summary>
        /// Xác định status dựa trên StartDate, EndDate và thời gian hiện tại
        /// </summary>
        private static CampaignStatus DetermineStatusByDate(DateTime? startDate, DateTime? endDate, DateTime currentTime)
        {
            // Nếu không có StartDate thì giữ Published (chưa xác định thời gian)
            if (!startDate.HasValue) return CampaignStatus.Published;

            // Nếu có EndDate và đã qua EndDate => Ended
            if (endDate.HasValue && currentTime > endDate.Value)
            {
                return CampaignStatus.Ended;
            }

            // Nếu đã qua StartDate nhưng chưa qua EndDate (hoặc không có EndDate) => Ongoing
            if (currentTime >= startDate.Value)
            {
                return CampaignStatus.Ongoing;
            }

            // Nếu chưa đến StartDate => Upcoming
            return CampaignStatus.Upcoming;
        }
    }
}