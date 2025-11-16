using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using CabinCrewCareer.Infrastructure.Hubs;

namespace CabinCrewCareer.Infrastructure.Services
{
    /// <summary>
    /// Service xử lý notifications
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider, IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _hubContext = hubContext;
        }

        public async Task SendTaskAssignmentNotificationAsync(int userId, string campaignName, string taskDescription, string assignedBy)
        {
            var title = "New Task";
            var body = $"You have been assigned a task: {taskDescription} in campaign '{campaignName}' by {assignedBy}.";
            var notification = await CreateNotificationAsync(userId, title, body, notificationTypeId: null);
            // Push realtime notification
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new {
                notification.NotificationId,
                notification.Title,
                notification.Body,
                Time = notification.Time.ToString("dd/MM/yyyy HH:mm"),
                notification.IsRead,
                notification.NotificationTypeId
            });
        }

        public async Task SendTaskCancellationNotificationAsync(int userId, string campaignName, string cancelReason, string cancelledBy)
        {
            var title = "Task Cancelled";
            var body = $"Your task in campaign '{campaignName}' has been cancelled by {cancelledBy}. Reason: {cancelReason}";
            var notification = await CreateNotificationAsync(userId, title, body, notificationTypeId: null);
            // Push realtime notification
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new {
                notification.NotificationId,
                notification.Title,
                notification.Body,
                Time = notification.Time.ToString("dd/MM/yyyy HH:mm"),
                notification.IsRead,
                notification.NotificationTypeId
            });
        }

        public async Task<Notification> CreateNotificationAsync(int userId, string title, string body, int? notificationTypeId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Body = body,
                Time = _dateTimeProvider.UtcNow,
                IsRead = false,
                NotificationTypeId = notificationTypeId
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            // KHÔNG gọi CompleteAsync() ở đây - để caller tự quyết định khi nào commit

            return notification;
        }
    }
}
