using FluentValidation;

namespace CabinCrewCareer.Application.TestSessions.Commands.SubmitSpeakingTest
{
    public class SubmitSpeakingTestCommandValidator : AbstractValidator<SubmitSpeakingTestCommand>
    {
        public SubmitSpeakingTestCommandValidator()
        {
            RuleFor(x => x.TestId)
                .GreaterThan(0)
                .WithMessage("TestId must be greater than 0");

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .WithMessage("StartTime is required");

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .WithMessage("EndTime is required")
                .GreaterThan(x => x.StartTime)
                .WithMessage("EndTime must be after StartTime");

            RuleFor(x => x.Answers)
                .NotEmpty()
                .WithMessage("At least one answer is required");

            RuleForEach(x => x.Answers)
                .ChildRules(answer =>
                {
                    answer.RuleFor(a => a.QuestionId)
                        .GreaterThan(0)
                        .WithMessage("QuestionId must be greater than 0");

                    answer.RuleFor(a => a.AudioFile)
                        .NotNull()
                        .WithMessage("Audio file is required");

                    answer.RuleFor(a => a.AudioFile.Length)
                        .LessThanOrEqualTo(52428800) // 50MB
                        .WithMessage("Audio file size must not exceed 50MB")
                        .When(a => a.AudioFile != null);

                    answer.RuleFor(a => a.AudioFile.ContentType)
                        .Must(contentType => contentType == "audio/mpeg" || contentType == "audio/mp3" || contentType == "audio/wav")
                        .WithMessage("Only .mp3 or .wav audio files are allowed")
                        .When(a => a.AudioFile != null);
                });
        }
    }
}
