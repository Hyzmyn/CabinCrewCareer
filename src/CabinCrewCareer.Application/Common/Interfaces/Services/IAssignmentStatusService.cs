namespace CabinCrewCareer.Application.Common.Interfaces.Services
{
    /// <summary>
    /// Service tự động cập nhật trạng thái của assignments dựa trên trạng thái campaign
    /// </summary>
    public interface IAssignmentStatusService
    {
        /// <summary>
        /// Cập nhật trạng thái assignments của một campaign dựa trên trạng thái campaign
        /// Assigned -> giữ nguyên khi Published
        /// Assigned -> InProgress khi campaign Ongoing
        /// InProgress -> Completed khi campaign Ended
        /// </summary>
        /// <param name="campaignId">ID của campaign</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateAssignmentStatusByCampaignStatusAsync(int campaignId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật trạng thái của tất cả assignments theo campaign status
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateAllAssignmentStatusesAsync(CancellationToken cancellationToken = default);
    }
}
