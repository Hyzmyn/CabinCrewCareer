using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignStatus
{
    public class UpdateCampaignStatusCommandHandler : IRequestHandler<UpdateCampaignStatusCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public UpdateCampaignStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Response<bool>> Handle(UpdateCampaignStatusCommand request, CancellationToken cancellationToken)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(request.CampaignId);
            if (campaign == null)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.CAMPAIGN_NOT_FOUND,
                    data: false,
                    message: ResponseCode.CAMPAIGN_NOT_FOUND.GetDescription());
            }

            // Kiểm tra trạng thái campaign
            if (campaign.CampaignStatus != CampaignStatus.Pending)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.FAIL,
                    data: false,
                    message: "Can only update status for campaigns with Pending status");
            }

            // Get username from token
            var currentUserName = _currentUserService.UserName;
            if (string.IsNullOrEmpty(currentUserName))
            {
                return new Response<bool>(
                    code: (int)ResponseCode.FAILED_AUTHENTICATION,
                    data: false,
                    message: "Unable to identify the approver");
            }

            if (request.Status == (int)CampaignStatus.Approved)
            {
                campaign.CampaignStatus = CampaignStatus.Approved;
                campaign.ReviewedBy = currentUserName; // Set reviewer username
                campaign.ApprovedAt = _dateTimeProvider.UtcNow; // Set approval time
                campaign.RejectedReason = string.Empty; // Clear reject reason if any
                campaign.RejectedAt = null; // Clear reject time if any
                
                await _unitOfWork.Campaigns.UpdateAsync(campaign);
                await _unitOfWork.CompleteAsync();
                
                return new Response<bool>(
                    code: (int)ResponseCode.SUCCESS,
                    data: true,
                    message: "Campaign approved successfully");
            }
            else if (request.Status == (int)CampaignStatus.Rejected)
            {
                // Check RejectReason
                if (string.IsNullOrWhiteSpace(request.RejectReason))
                {
                    return new Response<bool>(
                        code: (int)ResponseCode.FAIL,
                        data: false,
                        message: "Reject reason cannot be empty");
                }

                // Increment rejected count
                campaign.RejectedCount += 1;

                // Check if reached rejection limit (3 times)
                if (campaign.RejectedCount >= 3)
                {
                    // Cancel the campaign after 3 rejections
                    campaign.CampaignStatus = CampaignStatus.Canceled;
                    campaign.ReviewedBy = currentUserName; // Set reviewer username
                    campaign.RejectedAt = _dateTimeProvider.UtcNow;
                    campaign.RejectedReason = $"{request.RejectReason} (Campaign canceled after 3 rejections)";
                    campaign.ApprovedAt = null;
                    
                    await _unitOfWork.Campaigns.UpdateAsync(campaign);
                    await _unitOfWork.CompleteAsync();
                    
                    return new Response<bool>(
                        code: (int)ResponseCode.SUCCESS,
                        data: true,
                        message: "Campaign canceled after reaching rejection limit (3 times)");
                }
                else
                {
                    // Regular rejection (count < 3)
                    campaign.CampaignStatus = CampaignStatus.Rejected;
                    campaign.ReviewedBy = currentUserName; // Set reviewer username
                    campaign.RejectedAt = _dateTimeProvider.UtcNow;
                    campaign.RejectedReason = request.RejectReason;
                    campaign.ApprovedAt = null;
                    
                    await _unitOfWork.Campaigns.UpdateAsync(campaign);
                    await _unitOfWork.CompleteAsync();
                    
                    return new Response<bool>(
                        code: (int)ResponseCode.SUCCESS,
                        data: true,
                        message: $"Campaign rejected successfully ({campaign.RejectedCount}/3 rejections)");
                }
            }
            else
            {
                return new Response<bool>(
                    code: (int)ResponseCode.FAIL,
                    data: false,
                    message: "Invalid status. Can only approve (2) or reject (3)");
            }
        }
    }
}