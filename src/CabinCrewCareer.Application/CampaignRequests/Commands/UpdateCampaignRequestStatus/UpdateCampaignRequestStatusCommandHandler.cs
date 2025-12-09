using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.CampaignRequests.Commands.UpdateCampaignRequestStatus
{
    public class UpdateCampaignRequestStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider) : IRequestHandler<UpdateCampaignRequestStatusCommand, Response<GetCampaignRequestResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        public async Task<Response<GetCampaignRequestResponse>> Handle(UpdateCampaignRequestStatusCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new UpdateCampaignRequestStatusCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // Get current user from token (Director)
            var currentUserId = _currentUserService.CurrentPrincipal;
            var currentUserName = _currentUserService.UserName;
            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(currentUserName))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            // Get campaign request
            var campaignRequest = await _unitOfWork.CampaignRequests.GetByIdAsync(request.RequestId);
            if (campaignRequest == null)
            {
                errorMessages.Add(ResponseCode.CAMPAIGN_REQUEST_NOT_FOUND.GetDescription());
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // Check if request is already approved or rejected
            if (campaignRequest.Status != RequestStatus.Pending)
            {
                errorMessages.Add($"Cannot update status. Request is already {campaignRequest.Status}");
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // Update DirectorId
            campaignRequest.DirectorId = int.Parse(currentUserId);

            if (request.Status == (int)RequestStatus.Approved)
            {
                campaignRequest.Status = RequestStatus.Approved;
                campaignRequest.ApprovedAt = _dateTimeProvider.UtcNow;
                campaignRequest.RejectReason = string.Empty;

                // Tạo campaign mới với trạng thái draft
                var newCampaign = new CabinCrewCareer.Domain.Entities.Campaign
                {
                    CampaignName = campaignRequest.CampaignName,
                    Description = campaignRequest.Description,
                    JobDescription = campaignRequest.JobDescription,
                    JobRequirement = campaignRequest.JobRequirement,
                    TargetQuantity = campaignRequest.TargetQuantity,
                    CampaignType = campaignRequest.RequestType,
                    CampaignStatus = CabinCrewCareer.Domain.Common.CampaignStatus.Draft,
                    RequestId = campaignRequest.RequestId,
                    IsActive = true
                };
                await _unitOfWork.Campaigns.AddAsync(newCampaign);
            }
            else if (request.Status == (int)RequestStatus.Rejected)
            {
                // Increment rejected count
                campaignRequest.RejectedCount += 1;

                // Check if reached rejection limit (3 times)
                if (campaignRequest.RejectedCount >= 3)
                {
                    // Cancel the campaign request after 3 rejections
                    campaignRequest.Status = RequestStatus.Canceled;
                    campaignRequest.RejectedAt = _dateTimeProvider.UtcNow;
                    campaignRequest.RejectReason = $"{request.RejectReason ?? string.Empty} (Request canceled after 3 rejections)";
                }
                else
                {
                    // Regular rejection (count < 3)
                    campaignRequest.Status = RequestStatus.Rejected;
                    campaignRequest.RejectedAt = _dateTimeProvider.UtcNow;
                    campaignRequest.RejectReason = request.RejectReason ?? string.Empty;
                }
            }

            await _unitOfWork.CampaignRequests.UpdateAsync(campaignRequest);
            await _unitOfWork.CompleteAsync();

            // Load related entities (partner/director) for response
            var updatedRequestQuery = await _unitOfWork.CampaignRequests.GetAsync(
                cr => cr.RequestId == campaignRequest.RequestId,
                orderBy: null,
                includeFunc: q => q
                    .Include(cr => cr.AirlinePartner)
                    .Include(cr => cr.Director),
                disableTracking: true);

            var updatedRequest = await updatedRequestQuery.FirstOrDefaultAsync(cancellationToken);

            var response = new GetCampaignRequestResponse
            {
                RequestId = updatedRequest.RequestId,
                CampaignName = updatedRequest.CampaignName,
                Description = updatedRequest.Description,
                JobDescription = updatedRequest.JobDescription,
                JobRequirement = updatedRequest.JobRequirement,
                TargetQuantity = updatedRequest.TargetQuantity,
                RequestType = updatedRequest.RequestType.ToString(),
                Status = updatedRequest.Status.ToString(),
                RejectReason = updatedRequest.RejectReason,
                RejectedCount = updatedRequest.RejectedCount,
                ApprovedAt = updatedRequest.ApprovedAt?.GetFormatDateTime(),
                RejectedAt = updatedRequest.RejectedAt?.GetFormatDateTime(),
                PartnerName = updatedRequest.PartnerId != null ? updatedRequest.AirlinePartner?.PartnerName : null,
                DirectorName = updatedRequest.DirectorId != null ? updatedRequest.Director?.FullName : null,
                CreatedAt = (updatedRequest.CreatedAt ?? DateTime.UtcNow).GetFormatDateTime(),
                DueDate = updatedRequest.DueDate?.ToString()
            };

            return new Response<GetCampaignRequestResponse>(
                data: response,
                code: (int)ResponseCode.UPDATED_SUCCESS,
                message: ResponseCode.UPDATED_SUCCESS.ToString());
        }
    }
}
