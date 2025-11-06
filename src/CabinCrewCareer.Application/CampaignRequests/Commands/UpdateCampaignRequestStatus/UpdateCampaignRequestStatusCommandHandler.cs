using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

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
            if (string.IsNullOrEmpty(currentUserId))
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

            // Update status
            campaignRequest.Status = (RequestStatus)request.Status;
            campaignRequest.DirectorId = int.Parse(currentUserId);

            if (request.Status == (int)RequestStatus.Approved)
            {
                campaignRequest.ApprovedAt = _dateTimeProvider.UtcNow;
                campaignRequest.RejectReason = string.Empty;
            }
            else if (request.Status == (int)RequestStatus.Rejected)
            {
                campaignRequest.RejectedAt = _dateTimeProvider.UtcNow;
                campaignRequest.RejectReason = request.RejectReason ?? string.Empty;
            }

            await _unitOfWork.CampaignRequests.UpdateAsync(campaignRequest);
            await _unitOfWork.CompleteAsync();

            // Load related entities
            var updatedRequest = await _unitOfWork.CampaignRequests.GetByIdAsync(campaignRequest.RequestId);

            var response = new GetCampaignRequestResponse
            {
                RequestId = updatedRequest.RequestId,
                CampaignName = updatedRequest.CampaignName,
                Description = updatedRequest.Description,
                TargetQuantity = updatedRequest.TargetQuantity,
                Status = updatedRequest.Status.ToString(),
                RejectReason = updatedRequest.RejectReason,
                ApprovedAt = updatedRequest.ApprovedAt?.GetFormatDateTime(),
                RejectedAt = updatedRequest.RejectedAt?.GetFormatDateTime(),
                PartnerName = updatedRequest.AirlinePartner?.PartnerName,
                DirectorName = updatedRequest.Director?.FullName,
                CreatedAt = updatedRequest.CreatedAt ?? DateTime.UtcNow
            };

            return new Response<GetCampaignRequestResponse>(
                data: response,
                code: (int)ResponseCode.UPDATED_SUCCESS,
                message: ResponseCode.UPDATED_SUCCESS.ToString());
        }
    }
}
