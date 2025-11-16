using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Notifications.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Notifications.Queries.GetMyNotifications
{
    public class GetMyNotificationsQueryHandler 
        : IRequestHandler<GetMyNotificationsQuery, Response<List<NotificationDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetMyNotificationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Response<List<NotificationDto>>> Handle(
            GetMyNotificationsQuery request, 
            CancellationToken cancellationToken)
        {
            // Lấy userId từ token
            var currentUserIdString = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out var currentUserId))
            {
                return new Response<List<NotificationDto>>(
                    code: (int)ResponseCode.FAILED_AUTHENTICATION,
                    data: [],
                    message: ResponseCode.FAILED_AUTHENTICATION.GetDescription());
            }

            // Lấy tất cả notifications của user
            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            var notifications = allNotifications.Where(n => n.UserId == currentUserId);

            // Filter theo IsRead nếu có
            if (request.IsRead.HasValue)
            {
                notifications = notifications.Where(n => n.IsRead == request.IsRead.Value);
            }

            // Sắp xếp theo thời gian mới nhất và convert sang DTO
            var notificationList = notifications
                .OrderByDescending(n => n.Time)
                .ToList()
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Body = n.Body,
                    Time = n.Time.GetFormatDateTime(),
                    IsRead = n.IsRead
                })
                .ToList();

            return new Response<List<NotificationDto>>(
                code: (int)ResponseCode.SUCCESS,
                data: notificationList,
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
