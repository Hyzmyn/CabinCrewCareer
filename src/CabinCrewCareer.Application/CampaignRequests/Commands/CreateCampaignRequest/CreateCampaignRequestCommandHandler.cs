using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignRequests.Commands.CreateCampaignRequest
{
    public class CreateCampaignRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<CreateCampaignRequestCommand, Response<GetCampaignRequestResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<GetCampaignRequestResponse>> Handle(CreateCampaignRequestCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new CreateCampaignRequestCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Get current user from token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            // Get user to retrieve PartnerId
            var user = await _unitOfWork.Users.GetByIdAsync(int.Parse(currentUserId));
            if (user == null)
            {
                throw new ApiException(ResponseCode.USER_NOT_FOUND);
            }

            // Create campaign request
            var campaignRequest = new CampaignRequest
            {
                CampaignName = request.CampaignName,
                Description = request.Description,
                TargetQuantity = request.TargetQuantity,
                RequestType = request.RequestType,
                Status = RequestStatus.Pending,
                PartnerId = user.PartnerId,
                DirectorId = null,
                RejectReason = string.Empty
            };

            await _unitOfWork.CampaignRequests.AddAsync(campaignRequest);
            await _unitOfWork.CompleteAsync();

            // Load partner information
            var createdRequest = await _unitOfWork.CampaignRequests.GetByIdAsync(campaignRequest.RequestId);
            if (createdRequest == null)
            {
                throw new ApiException(ResponseCode.CREATED_UNSUCC);
            }

            var response = new GetCampaignRequestResponse
            {
                RequestId = createdRequest.RequestId,
                CampaignName = createdRequest.CampaignName,
                Description = createdRequest.Description,
                TargetQuantity = createdRequest.TargetQuantity,
                RequestType = createdRequest.RequestType.ToString(),
                Status = createdRequest.Status.ToString(),
                RejectReason = createdRequest.RejectReason,
                ApprovedAt = createdRequest.ApprovedAt?.GetFormatDateTime(),
                RejectedAt = createdRequest.RejectedAt?.GetFormatDateTime(),
                PartnerName = createdRequest.AirlinePartner?.PartnerName,
                DirectorName = createdRequest.Director?.FullName,
                CreatedAt = createdRequest.CreatedAt ?? DateTime.UtcNow
            };

            return new Response<GetCampaignRequestResponse>(
                data: response,
                code: (int)ResponseCode.CREATED_SUCCESS,
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
