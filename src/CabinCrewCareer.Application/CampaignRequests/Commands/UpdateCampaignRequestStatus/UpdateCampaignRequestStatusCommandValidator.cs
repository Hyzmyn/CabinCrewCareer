using CabinCrewCareer.Domain.Common;
using FluentValidation;

namespace CabinCrewCareer.Application.CampaignRequests.Commands.UpdateCampaignRequestStatus
{
    public class UpdateCampaignRequestStatusCommandValidator : AbstractValidator<UpdateCampaignRequestStatusCommand>
    {
        public UpdateCampaignRequestStatusCommandValidator()
        {
            RuleFor(x => x.RequestId)
                .GreaterThan(0).WithMessage("Request ID must be greater than 0");

            RuleFor(x => x.Status)
                .Must(status => status == (int)RequestStatus.Approved || status == (int)RequestStatus.Rejected)
                .WithMessage("Status must be Approved (2) or Rejected (3)");

            RuleFor(x => x.RejectReason)
                .NotEmpty().WithMessage("Reject reason is required when rejecting a request")
                .MaximumLength(500).WithMessage("Reject reason must not exceed 500 characters")
                .When(x => x.Status == (int)RequestStatus.Rejected);
        }
    }
}
