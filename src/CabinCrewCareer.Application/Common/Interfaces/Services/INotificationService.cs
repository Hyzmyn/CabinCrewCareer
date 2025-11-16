using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Services
{
    /// <summary>
    /// Service quản lý notifications
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Gửi notification cho user khi được giao task
        /// </summary>
        /// <param name="userId">ID của user nhận notification</param>
        /// <param name="campaignName">Tên campaign</param>
        /// <param name="taskDescription">Mô tả task</param>
        /// <param name="assignedBy">Người giao việc</param>
        Task SendTaskAssignmentNotificationAsync(int userId, string campaignName, string taskDescription, string assignedBy);

        /// <summary>
        /// Gửi notification khi assignment bị cancel
        /// </summary>
        /// <param name="userId">ID của user nhận notification</param>
        /// <param name="campaignName">Tên campaign</param>
        /// <param name="cancelReason">Lý do cancel</param>
        /// <param name="cancelledBy">Người cancel</param>
        Task SendTaskCancellationNotificationAsync(int userId, string campaignName, string cancelReason, string cancelledBy);

        /// <summary>
        /// Tạo notification và lưu vào database
        /// </summary>
        Task<Notification> CreateNotificationAsync(int userId, string title, string body, int? notificationTypeId = null);
    }
}
