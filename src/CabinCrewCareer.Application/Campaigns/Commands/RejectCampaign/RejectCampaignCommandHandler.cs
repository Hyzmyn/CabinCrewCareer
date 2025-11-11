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
                return new Response<bool>
                {
                    Code = 404,
                    Message = "Không tìm thấy chiến dịch",
                    Data = false
                };
            }

            // Kiểm tra trạng thái campaign
            if (campaign.CampaignStatus != CampaignStatus.Pending)
            {
                return new Response<bool>
                {
                    Code = 400,
                    Message = "Chỉ có thể từ chối chiến dịch có trạng thái Pending",
                    Data = false
                };
            }

            // Kiểm tra RejectReason
            if (string.IsNullOrWhiteSpace(request.RejectReason))
            {
                return new Response<bool>
                {
                    Code = 400,
                    Message = "Lý do từ chối không được để trống",
                    Data = false
                };
            }

            // Lấy DirectorId từ token (để log ai từ chối nếu cần)
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return new Response<bool>
                {
                    Code = 401,
                    Message = "Không xác định được người từ chối",
                    Data = false
                };
            }

            // Cập nhật trạng thái campaign
            campaign.CampaignStatus = CampaignStatus.Rejected;
            campaign.RejectedReason = request.RejectReason;

            await _unitOfWork.Campaigns.UpdateAsync(campaign);
            await _unitOfWork.CompleteAsync();

            return new Response<bool>
            {
                Code = 200,
                Message = "Từ chối chiến dịch thành công",
                Data = true
            };
        }
    }
}