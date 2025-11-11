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
                    Message = "Chỉ có thể duyệt chiến dịch có trạng thái Pending",
                    Data = false
                };
            }

            // Lấy DirectorId từ token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return new Response<bool>
                {
                    Code = 401,
                    Message = "Không xác định được người duyệt",
                    Data = false
                };
            }

            // Cập nhật trạng thái campaign
            campaign.CampaignStatus = CampaignStatus.Approved;
            // Không cần DirectorId vì Campaign entity không có field này
            campaign.RejectedReason = string.Empty; // Xóa reject reason nếu có

            await _unitOfWork.Campaigns.UpdateAsync(campaign);
            await _unitOfWork.CompleteAsync();

            return new Response<bool>
            {
                Code = 200,
                Message = "Duyệt chiến dịch thành công",
                Data = true
            };
        }
    }
}