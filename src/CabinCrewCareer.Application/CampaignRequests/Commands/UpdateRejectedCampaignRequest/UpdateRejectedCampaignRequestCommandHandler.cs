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

namespace CabinCrewCareer.Application.CampaignRequests.Commands.UpdateRejectedCampaignRequest
{
    public class UpdateRejectedCampaignRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<UpdateRejectedCampaignRequestCommand, Response<GetCampaignRequestResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<GetCampaignRequestResponse>> Handle(UpdateRejectedCampaignRequestCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new UpdateRejectedCampaignRequestCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // Get current user from token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            // Get user to verify PartnerId
            var user = await _unitOfWork.Users.GetByIdAsync(int.Parse(currentUserId));
            if (user == null)
            {
                throw new ApiException(ResponseCode.USER_NOT_FOUND);
            }

            // Get campaign request
            var campaignRequest = await _unitOfWork.CampaignRequests.GetByIdAsync(request.RequestId);
            if (campaignRequest == null)
            {
                throw new ApiException(ResponseCode.CAMPAIGN_REQUEST_NOT_FOUND);
            }

            // Verify ownership - user must be from the same partner
            if (campaignRequest.PartnerId != user.PartnerId)
            {
                errorMessages.Add("You do not have permission to update this request");
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // Check if request is in Rejected status
            if (campaignRequest.Status != RequestStatus.Rejected)
            {
                errorMessages.Add("Can only update rejected campaign requests");
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // Check if request is canceled (reached rejection limit)
            if (campaignRequest.RejectedCount >= 3)
            {
                errorMessages.Add("Campaign request has been canceled after 3 rejections and cannot be resubmitted");
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // Update campaign request
            campaignRequest.CampaignName = request.CampaignName;
            campaignRequest.Description = request.Description;
            campaignRequest.JobDescription = request.JobDescription;
            campaignRequest.JobRequirement = request.JobRequirement;
            campaignRequest.TargetQuantity = request.TargetQuantity;
            campaignRequest.Status = RequestStatus.Pending;
            campaignRequest.RejectReason = string.Empty;
            campaignRequest.RejectedAt = null;
            campaignRequest.DirectorId = null;

            await _unitOfWork.CampaignRequests.UpdateAsync(campaignRequest);
            await _unitOfWork.CompleteAsync();

            // Reload with navigation properties
            var updatedRequestQuery = await _unitOfWork.CampaignRequests.GetAsync(
                cr => cr.RequestId == campaignRequest.RequestId,
                orderBy: null,
                includeFunc: q => q
                    .Include(cr => cr.AirlinePartner)
                    .Include(cr => cr.Director),
                disableTracking: true);

            var updatedRequest = await updatedRequestQuery.FirstOrDefaultAsync(cancellationToken);
            if (updatedRequest == null)
            {
                throw new ApiException(ResponseCode.UPDATED_UNSUCC);
            }

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
                CreatedAt = (updatedRequest.CreatedAt ?? DateTime.UtcNow).GetFormatDateTime()
            };

            return new Response<GetCampaignRequestResponse>(
                data: response,
                code: (int)ResponseCode.UPDATED_SUCCESS,
                message: ResponseCode.UPDATED_SUCCESS.GetDescription());
        }
    }
}
