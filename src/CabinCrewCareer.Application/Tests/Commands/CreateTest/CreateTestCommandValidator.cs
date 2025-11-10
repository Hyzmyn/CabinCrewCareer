using CabinCrewCareer.Domain.Common;
using FluentValidation;

namespace CabinCrewCareer.Application.Tests.Commands.CreateTest
{
    public class CreateTestCommandValidator : AbstractValidator<CreateTestCommand>
    {
        public CreateTestCommandValidator()
        {
            RuleFor(x => x.TestName)
                .NotEmpty().WithMessage("Test name is required")
                .MaximumLength(200).WithMessage("Test name must not exceed 200 characters");

            RuleFor(x => x.Purpose)
                .NotEmpty().WithMessage("Purpose is required")
                .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters");

            RuleFor(x => x.TestType)
                .IsInEnum().WithMessage("Invalid test type");

            RuleFor(x => x.MaxScore)
                .GreaterThan(0).WithMessage("Max score must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Max score must not exceed 1000");

            // Nếu TestType là English thì AudioFileURL là bắt buộc
            RuleFor(x => x.AudioFileURL)
                .NotEmpty()
                .WithMessage("Audio file URL is required for English test type")
                .When(x => x.TestType == TestType.English);

            // Nếu có AudioFileURL thì phải là URL hợp lệ
            RuleFor(x => x.AudioFileURL)
                .Must(BeAValidUrl)
                .WithMessage("Audio file URL must be a valid URL")
                .When(x => !string.IsNullOrEmpty(x.AudioFileURL));
        }

        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
