using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Notifications.Commands.MarkNotificationAsRead
{
    public class MarkNotificationAsReadCommandHandler 
        : IRequestHandler<MarkNotificationAsReadCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Response<bool>> Handle(
            MarkNotificationAsReadCommand request, 
            CancellationToken cancellationToken)
        {
            // Lấy userId từ token
            var currentUserIdString = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out var currentUserId))
            {
                return new Response<bool>(
                    code: (int)ResponseCode.FAILED_AUTHENTICATION,
                    data: false,
                    message: ResponseCode.FAILED_AUTHENTICATION.GetDescription());
            }

            // Lấy notification
            var notification = await _unitOfWork.Notifications.GetByIdAsync(request.NotificationId);
            if (notification == null)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.NOTIFICATION_NOT_FOUND,
                    data: false,
                    message: ResponseCode.NOTIFICATION_NOT_FOUND.GetDescription());
            }

            // Kiểm tra notification có thuộc về user không
            if (notification.UserId != currentUserId)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.NOTIFICATION_ACCESS_DENIED,
                    data: false,
                    message: ResponseCode.NOTIFICATION_ACCESS_DENIED.GetDescription());
            }

            // Đánh dấu đã đọc
            notification.IsRead = true;
            await _unitOfWork.Notifications.UpdateAsync(notification);
            await _unitOfWork.CompleteAsync();

            return new Response<bool>(
                code: (int)ResponseCode.UPDATED_SUCCESS,
                data: true,
                message: ResponseCode.UPDATED_SUCCESS.GetDescription());
        }
    }
}
