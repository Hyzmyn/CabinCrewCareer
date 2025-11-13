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

            RuleFor(x => x.DurationInMinutes)
                .GreaterThan(0).WithMessage("Duration must be greater than 0 minutes")
                .LessThanOrEqualTo(480).WithMessage("Duration must not exceed 8 hours (480 minutes)");

            // Nếu TestType là EnglishListening thì AudioFile là bắt buộc
            RuleFor(x => x.AudioFile)
                .NotNull()
                .WithMessage("Audio file is required for English Listening test type")
                .When(x => x.TestType == TestType.EnglishListening);

            // Validate file extension và size nếu có AudioFile
            RuleFor(x => x.AudioFile)
                .Must(BeValidAudioFile)
                .WithMessage("Audio file must be .mp3 or .wav format and not exceed 50 MB")
                .When(x => x.AudioFile != null);
        }

        private bool BeValidAudioFile(Microsoft.AspNetCore.Http.IFormFile? file)
        {
            if (file == null)
                return true;

            // Check file extension
            var allowedExtensions = new[] { ".mp3", ".wav" };
            var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
                return false;

            // Check file size (max 50 MB)
            const long maxFileSize = 50 * 1024 * 1024; // 50 MB
            if (file.Length > maxFileSize)
                return false;

            return true;
        }
    }
}
