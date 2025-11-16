using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Commands.ApproveCampaign
{
    public class ApproveCampaignCommandHandler : IRequestHandler<ApproveCampaignCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ApproveCampaignCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Response<bool>> Handle(ApproveCampaignCommand request, CancellationToken cancellationToken)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(request.CampaignId);
            if (campaign == null)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.CAMPAIGN_NOT_FOUND,
                    data: false,
                    message: "Không tìm thấy chiến dịch");
            }

            // Kiểm tra trạng thái campaign
            if (campaign.CampaignStatus != CampaignStatus.Pending)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.FAIL,
                    data: false,
                    message: "Chỉ có thể duyệt chiến dịch có trạng thái Pending");
            }

            // Lấy DirectorId từ token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return new Response<bool>(
                    code: (int)ResponseCode.FAILED_AUTHENTICATION,
                    data: false,
                    message: "Không xác định được người duyệt");
            }

            // Cập nhật trạng thái campaign
            campaign.CampaignStatus = CampaignStatus.Approved;
            // Không cần DirectorId vì Campaign entity không có field này
            campaign.RejectedReason = string.Empty; // Xóa reject reason nếu có

            await _unitOfWork.Campaigns.UpdateAsync(campaign);
            await _unitOfWork.CompleteAsync();

            return new Response<bool>(
                code: (int)ResponseCode.SUCCESS,
                data: true,
                message: "Duyệt chiến dịch thành công");
        }
    }
}