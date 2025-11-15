using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignAssignments.Commands.UpdateTaskStatus
{
    public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateTaskStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Response<bool>> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
        {
            // Lấy userId từ token
            var currentUserIdString = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out var currentUserId))
            {
                return new Response<bool>
                {
                    Code = 401,
                    Message = "Không xác định được người dùng hiện tại",
                    Data = false
                };
            }

            // Lấy assignment
            var assignment = await _unitOfWork.CampaignAssignments.GetByIdAsync(request.AssignmentId);
            if (assignment == null)
            {
                return new Response<bool>
                {
                    Code = 404,
                    Message = "Không tìm thấy task",
                    Data = false
                };
            }

            // Kiểm tra quyền - chỉ owner của task mới được cập nhật
            if (assignment.UserId != currentUserId)
            {
                return new Response<bool>
                {
                    Code = 403,
                    Message = "Bạn không có quyền cập nhật task này",
                    Data = false
                };
            }

            // Validate transition hợp lệ
            var isValidTransition = IsValidStatusTransition(assignment.Status, request.Status);
            if (!isValidTransition)
            {
                return new Response<bool>
                {
                    Code = 400,
                    Message = $"Không thể chuyển từ {GetStatusText(assignment.Status)} sang {GetStatusText(request.Status)}",
                    Data = false
                };
            }

            // Cập nhật trạng thái
            assignment.Status = request.Status;
            
            // Cập nhật CompletedAt nếu status là Completed
            if (request.Status == AssignmentStatus.Completed)
            {
                assignment.CompletedAt = DateTime.UtcNow;
            }

            await _unitOfWork.CampaignAssignments.UpdateAsync(assignment);
            await _unitOfWork.CompleteAsync();

            return new Response<bool>
            {
                Code = 200,
                Message = "Cập nhật trạng thái task thành công",
                Data = true
            };
        }

        private static bool IsValidStatusTransition(AssignmentStatus currentStatus, AssignmentStatus newStatus)
        {
            return currentStatus switch
            {
                AssignmentStatus.Assigned => newStatus == AssignmentStatus.InProgress || newStatus == AssignmentStatus.Cancelled,
                AssignmentStatus.InProgress => newStatus == AssignmentStatus.Completed || newStatus == AssignmentStatus.Cancelled,
                AssignmentStatus.Completed => false, // Không thể thay đổi từ Completed
                AssignmentStatus.Cancelled => false, // Không thể thay đổi từ Cancelled
                _ => false
            };
        }

        private static string GetStatusText(AssignmentStatus status)
        {
            return status switch
            {
                AssignmentStatus.Assigned => "Assigned",
                AssignmentStatus.InProgress => "In Progress",
                AssignmentStatus.Completed => "Completed",
                AssignmentStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}