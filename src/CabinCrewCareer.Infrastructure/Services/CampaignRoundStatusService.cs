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

            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(campaignRound.CampaignId);
            if (campaign == null) return;

            // Nếu campaign đang Pending, Approved hoặc Rejected, round luôn là Upcoming
            if (campaign.CampaignStatus == CampaignStatus.Pending || campaign.CampaignStatus == CampaignStatus.Approved || campaign.CampaignStatus == CampaignStatus.Rejected)
            {
                if (campaignRound.RoundStatus != RoundStatus.Upcoming)
                {
                    campaignRound.RoundStatus = RoundStatus.Upcoming;
                    await _unitOfWork.CampaignRounds.UpdateAsync(campaignRound);
                    await _unitOfWork.CompleteAsync();
                }
                return;
            }

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
            // Lấy tất cả campaign active
            var campaigns = await _unitOfWork.Campaigns.GetAllAsync(c => c.IsActive);
            var currentTime = _dateTimeProvider.UtcNow;
            bool hasChanges = false;

            foreach (var campaign in campaigns)
            {
                var rounds = campaign.CampaignRounds.Where(r => r.IsActive).OrderBy(r => r.StartDate).ToList();
                if (!rounds.Any()) continue;

                // Nếu campaign đang Pending hoặc Approved, tất cả round phải là Upcoming
                if (campaign.CampaignStatus == CampaignStatus.Pending || campaign.CampaignStatus == CampaignStatus.Approved)
                {
                    foreach (var round in rounds)
                    {
                        if (round.RoundStatus != RoundStatus.Upcoming)
                        {
                            round.RoundStatus = RoundStatus.Upcoming;
                            await _unitOfWork.CampaignRounds.UpdateAsync(round);
                            hasChanges = true;
                        }
                    }
                    continue;
                }

                // Nếu campaign Ongoing
                if (campaign.CampaignStatus == CampaignStatus.Ongoing)
                {
                    for (int i = 0; i < rounds.Count; i++)
                    {
                        var round = rounds[i];
                        if (i == 0)
                        {
                            // Round đầu tiên: chuyển sang Ongoing nếu thời gian >= StartDate
                            if (currentTime >= round.StartDate && round.RoundStatus != RoundStatus.Ongoing)
                            {
                                round.RoundStatus = RoundStatus.Ongoing;
                                await _unitOfWork.CampaignRounds.UpdateAsync(round);
                                hasChanges = true;
                            }
                            else if (currentTime < round.StartDate && round.RoundStatus != RoundStatus.Upcoming)
                            {
                                round.RoundStatus = RoundStatus.Upcoming;
                                await _unitOfWork.CampaignRounds.UpdateAsync(round);
                                hasChanges = true;
                            }
                        }
                        else
                        {
                            // Các round còn lại: chỉ chuyển sang Ongoing nếu thời gian >= StartDate
                            if (currentTime >= round.StartDate && (round.RoundStatus == RoundStatus.Upcoming || round.RoundStatus == RoundStatus.Ongoing))
                            {
                                round.RoundStatus = RoundStatus.Ongoing;
                                await _unitOfWork.CampaignRounds.UpdateAsync(round);
                                hasChanges = true;
                            }
                            else if (currentTime < round.StartDate && round.RoundStatus != RoundStatus.Upcoming)
                            {
                                round.RoundStatus = RoundStatus.Upcoming;
                                await _unitOfWork.CampaignRounds.UpdateAsync(round);
                                hasChanges = true;
                            }
                        }
                        // Nếu đã qua EndDate thì chuyển sang Ended
                        if (round.EndDate.HasValue && currentTime > round.EndDate.Value && round.RoundStatus != RoundStatus.Ended)
                        {
                            round.RoundStatus = RoundStatus.Ended;
                            await _unitOfWork.CampaignRounds.UpdateAsync(round);
                            hasChanges = true;
                        }
                    }
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