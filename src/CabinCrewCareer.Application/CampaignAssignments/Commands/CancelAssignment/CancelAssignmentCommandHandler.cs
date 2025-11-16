using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignAssignments.Commands.CancelAssignment
{
    /// <summary>
    /// Handler cho CancelAssignmentCommand - Chỉ Senior Recruiter mới được cancel assignment
    /// </summary>
    public class CancelAssignmentCommandHandler : IRequestHandler<CancelAssignmentCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;

        public CancelAssignmentCommandHandler(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUserService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        public async Task<Response<bool>> Handle(CancelAssignmentCommand request, CancellationToken cancellationToken)
        {
            // Get current user from token
            var currentUserIdString = _currentUserService.CurrentPrincipal;
            var currentUserName = _currentUserService.UserName;
            
            if (string.IsNullOrEmpty(currentUserIdString) || string.IsNullOrEmpty(currentUserName))
            {
                return new Response<bool>(
                    code: (int)ResponseCode.FAILED_AUTHENTICATION,
                    data: false,
                    message: ResponseCode.FAILED_AUTHENTICATION.GetDescription());
            }

            if (!int.TryParse(currentUserIdString, out var currentUserId))
            {
                return new Response<bool>(
                    code: (int)ResponseCode.FAILED_AUTHENTICATION,
                    data: false,
                    message: ResponseCode.FAILED_AUTHENTICATION.GetDescription());
            }

            // Lấy assignment
            var assignment = await _unitOfWork.CampaignAssignments.GetByIdAsync(request.AssignmentId);
            if (assignment == null)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.ASSIGNMENT_NOT_FOUND,
                    data: false,
                    message: ResponseCode.ASSIGNMENT_NOT_FOUND.GetDescription());
            }

            // Kiểm tra assignment đã completed hay cancelled chưa
            if (assignment.Status == AssignmentStatus.Completed)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.CANNOT_CANCEL_COMPLETED_ASSIGNMENT,
                    data: false,
                    message: ResponseCode.CANNOT_CANCEL_COMPLETED_ASSIGNMENT.GetDescription());
            }

            if (assignment.Status == AssignmentStatus.Cancelled)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.ASSIGNMENT_ALREADY_CANCELLED,
                    data: false,
                    message: ResponseCode.ASSIGNMENT_ALREADY_CANCELLED.GetDescription());
            }

            // Lấy thông tin campaign để gửi notification
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(assignment.CampaignId ?? 0);
            if (campaign == null)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.CAMPAIGN_NOT_FOUND,
                    data: false,
                    message: ResponseCode.CAMPAIGN_NOT_FOUND.GetDescription());
            }

            // Cập nhật status thành Cancelled
            assignment.Status = AssignmentStatus.Cancelled;
            assignment.Notes = $"Cancelled by {currentUserName}. Reason: {request.CancelReason}";

            await _unitOfWork.CampaignAssignments.UpdateAsync(assignment);
            await _unitOfWork.CompleteAsync();

            // Gửi notification cho user bị cancel assignment
            if (assignment.UserId.HasValue)
            {
                await _notificationService.SendTaskCancellationNotificationAsync(
                    userId: assignment.UserId.Value,
                    campaignName: campaign.CampaignName,
                    cancelReason: request.CancelReason,
                    cancelledBy: currentUserName
                );
            }

            return new Response<bool>(
                code: (int)ResponseCode.UPDATED_SUCCESS,
                data: true,
                message: ResponseCode.UPDATED_SUCCESS.GetDescription());
        }
    }
}
