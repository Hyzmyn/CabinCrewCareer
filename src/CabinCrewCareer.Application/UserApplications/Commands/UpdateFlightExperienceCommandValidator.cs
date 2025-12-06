using FluentValidation;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class UpdateFlightExperienceCommandValidator : AbstractValidator<UpdateFlightExperienceCommand>
    {
        public UpdateFlightExperienceCommandValidator()
        {
            RuleFor(x => x.ActivityId)
                .GreaterThan(0)
                .WithMessage("Application Snapshot ID must be greater than 0.");

            RuleFor(x => x.TotalFlightHours)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Total Flight Hours must be greater than or equal to 0.");

            RuleFor(x => x.Experience)
                .MaximumLength(500)
                .WithMessage("Experience must not exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Experience));
        }
    }
}
