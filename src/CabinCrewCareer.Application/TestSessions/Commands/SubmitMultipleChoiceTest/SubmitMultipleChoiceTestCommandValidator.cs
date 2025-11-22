using FluentValidation;

namespace CabinCrewCareer.Application.TestSessions.Commands.SubmitMultipleChoiceTest
{
    public class SubmitMultipleChoiceTestCommandValidator : AbstractValidator<SubmitMultipleChoiceTestCommand>
    {
        public SubmitMultipleChoiceTestCommandValidator()
        {
            RuleFor(x => x.TestId)
                .GreaterThan(0)
                .WithMessage("Test ID must be greater than 0");

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .WithMessage("Start time is required")
                .LessThan(x => x.EndTime)
                .WithMessage("Start time must be before end time");

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .WithMessage("End time is required")
                .GreaterThan(x => x.StartTime)
                .WithMessage("End time must be after start time");

            RuleFor(x => x.Answers)
                .NotEmpty()
                .WithMessage("At least one answer is required")
                .Must(answers => answers != null && answers.Count > 0)
                .WithMessage("Answers list cannot be empty");

            RuleForEach(x => x.Answers).ChildRules(answer =>
            {
                answer.RuleFor(a => a.QuestionId)
                    .GreaterThan(0)
                    .WithMessage("Question ID must be greater than 0");

                answer.RuleFor(a => a.SelectedOptionId)
                    .GreaterThan(0)
                    .WithMessage("Selected option ID must be greater than 0");
            });
        }
    }
}
