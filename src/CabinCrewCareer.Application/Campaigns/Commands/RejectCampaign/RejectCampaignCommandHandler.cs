using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Commands.RejectCampaign
{
    public class RejectCampaignCommandHandler : IRequestHandler<RejectCampaignCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public RejectCampaignCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Response<bool>> Handle(RejectCampaignCommand request, CancellationToken cancellationToken)
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
                    message: "Chỉ có thể từ chối chiến dịch có trạng thái Pending");
            }

            // Kiểm tra RejectReason
            if (string.IsNullOrWhiteSpace(request.RejectReason))
            {
                return new Response<bool>(
                    code: (int)ResponseCode.FAIL,
                    data: false,
                    message: "Lý do từ chối không được để trống");
            }

            // Lấy DirectorId từ token (để log ai từ chối nếu cần)
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return new Response<bool>(
                    code: (int)ResponseCode.FAILED_AUTHENTICATION,
                    data: false,
                    message: "Không xác định được người từ chối");
            }

            // Cập nhật trạng thái campaign
            campaign.CampaignStatus = CampaignStatus.Rejected;
            campaign.RejectedReason = request.RejectReason;

            await _unitOfWork.Campaigns.UpdateAsync(campaign);
            await _unitOfWork.CompleteAsync();

            return new Response<bool>(
                code: (int)ResponseCode.SUCCESS,
                data: true,
                message: "Từ chối chiến dịch thành công");
        }
    }
}