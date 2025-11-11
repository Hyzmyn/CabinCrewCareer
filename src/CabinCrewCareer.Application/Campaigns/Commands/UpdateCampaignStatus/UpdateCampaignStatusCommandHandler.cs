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
                return new Response<bool>
                {
                    Code = 404,
                    Message = "Campaign not found",
                    Data = false
                };
            }

            // Kiểm tra trạng thái campaign
            if (campaign.CampaignStatus != CampaignStatus.Pending)
            {
                return new Response<bool>
                {
                    Code = 400,
                    Message = "Can only update status for campaigns with Pending status",
                    Data = false
                };
            }

            // Get DirectorId from token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return new Response<bool>
                {
                    Code = 401,
                    Message = "Unable to identify the approver",
                    Data = false
                };
            }

            if (request.Status == (int)CampaignStatus.Approved)
            {
                campaign.CampaignStatus = CampaignStatus.Approved;
                campaign.ReviewedBy = currentUserId; // Set reviewer ID
                campaign.ApprovedAt = _dateTimeProvider.UtcNow; // Set approval time
                campaign.RejectedReason = string.Empty; // Clear reject reason if any
                campaign.RejectedAt = null; // Clear reject time if any
                
                await _unitOfWork.Campaigns.UpdateAsync(campaign);
                await _unitOfWork.CompleteAsync();
                
                return new Response<bool>
                {
                    Code = 200,
                    Message = "Campaign approved successfully",
                    Data = true
                };
            }
            else if (request.Status == (int)CampaignStatus.Rejected)
            {
                // Check RejectReason
                if (string.IsNullOrWhiteSpace(request.RejectReason))
                {
                    return new Response<bool>
                    {
                        Code = 400,
                        Message = "Reject reason cannot be empty",
                        Data = false
                    };
                }

                campaign.CampaignStatus = CampaignStatus.Rejected;
                campaign.ReviewedBy = currentUserId; // Set reviewer ID
                campaign.RejectedAt = _dateTimeProvider.UtcNow; // Set rejection time
                campaign.RejectedReason = request.RejectReason;
                campaign.ApprovedAt = null; // Clear approval time if any
                
                await _unitOfWork.Campaigns.UpdateAsync(campaign);
                await _unitOfWork.CompleteAsync();
                
                return new Response<bool>
                {
                    Code = 200,
                    Message = "Campaign rejected successfully",
                    Data = true
                };
            }
            else
            {
                return new Response<bool>
                {
                    Code = 400,
                    Message = "Invalid status. Can only approve (2) or reject (3)",
                    Data = false
                };
            }
        }
    }
}