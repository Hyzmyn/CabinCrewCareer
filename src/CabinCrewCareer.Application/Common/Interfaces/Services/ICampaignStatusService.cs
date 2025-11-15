namespace CabinCrewCareer.Application.Common.Interfaces.Services
{
    public interface ICampaignStatusService
    {
        /// <summary>
        /// Cập nhật trạng thái campaign dựa trên StartDate và EndDate
        /// </summary>
        /// <param name="campaignId">ID của campaign</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateCampaignStatusByDateAsync(int campaignId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật trạng thái của tất cả campaigns dựa trên thời gian hiện tại
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateAllCampaignStatusesAsync(CancellationToken cancellationToken = default);
    }

    public interface ICampaignRoundStatusService
    {
        /// <summary>
        /// Cập nhật trạng thái campaign round dựa trên StartDate và EndDate
        /// </summary>
        /// <param name="campaignRoundId">ID của campaign round</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateCampaignRoundStatusByDateAsync(int campaignRoundId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật trạng thái của tất cả campaign rounds dựa trên thời gian hiện tại
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateAllCampaignRoundStatusesAsync(CancellationToken cancellationToken = default);
    }
}