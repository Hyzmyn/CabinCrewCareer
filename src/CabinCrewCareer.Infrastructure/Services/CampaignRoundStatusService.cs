using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Infrastructure.Services
{
    public class CampaignRoundStatusService : ICampaignRoundStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CampaignRoundStatusService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task UpdateCampaignRoundStatusByDateAsync(int campaignRoundId, CancellationToken cancellationToken = default)
        {
            var campaignRound = await _unitOfWork.CampaignRounds.GetByIdAsync(campaignRoundId);
            if (campaignRound == null) return;

            var currentTime = _dateTimeProvider.UtcNow;
            var newStatus = DetermineStatusByDate(campaignRound.StartDate, campaignRound.EndDate, currentTime);

            // Chỉ cập nhật nếu status thực sự thay đổi
            if (newStatus != campaignRound.RoundStatus)
            {
                campaignRound.RoundStatus = newStatus;
                await _unitOfWork.CampaignRounds.UpdateAsync(campaignRound);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task UpdateAllCampaignRoundStatusesAsync(CancellationToken cancellationToken = default)
        {
            // Lấy tất cả campaign rounds active
            var campaignRounds = await _unitOfWork.CampaignRounds.GetAllAsync();
            var roundsToUpdate = campaignRounds.Where(r => r.IsActive).ToList();

            if (!roundsToUpdate.Any()) return;

            var currentTime = _dateTimeProvider.UtcNow;
            bool hasChanges = false;

            foreach (var round in roundsToUpdate)
            {
                var newStatus = DetermineStatusByDate(round.StartDate, round.EndDate, currentTime);
                
                if (newStatus != round.RoundStatus)
                {
                    round.RoundStatus = newStatus;
                    await _unitOfWork.CampaignRounds.UpdateAsync(round);
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
        private static RoundStatus DetermineStatusByDate(DateTime? startDate, DateTime? endDate, DateTime currentTime)
        {
            // Nếu không có StartDate thì giữ Upcoming (chưa xác định thời gian)
            if (!startDate.HasValue) return RoundStatus.Upcoming;

            // Nếu có EndDate và đã qua EndDate => Ended
            if (endDate.HasValue && currentTime > endDate.Value)
            {
                return RoundStatus.Ended;
            }

            // Nếu đã qua StartDate nhưng chưa qua EndDate (hoặc không có EndDate) => Ongoing
            if (currentTime >= startDate.Value)
            {
                return RoundStatus.Ongoing;
            }

            // Nếu chưa đến StartDate => Upcoming
            return RoundStatus.Upcoming;
        }
    }
}