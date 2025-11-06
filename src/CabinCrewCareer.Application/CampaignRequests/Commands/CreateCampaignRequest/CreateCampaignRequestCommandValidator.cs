using FluentValidation;

namespace CabinCrewCareer.Application.CampaignRequests.Commands.CreateCampaignRequest
{
    public class CreateCampaignRequestCommandValidator : AbstractValidator<CreateCampaignRequestCommand>
    {
        public CreateCampaignRequestCommandValidator()
        {
            RuleFor(x => x.CampaignName)
                .NotEmpty().WithMessage("Campaign name is required")
                .MaximumLength(200).WithMessage("Campaign name must not exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.TargetQuantity)
                .GreaterThan(0).WithMessage("Target quantity must be greater than 0");

            RuleFor(x => x.RequestType)
                .IsInEnum().WithMessage("Request type must be valid (1 = Recruitment, 2 = Promotion)");
        }
    }
}
