using FluentValidation;
using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Application.TestQuestions.Commands.CreateTestQuestion
{
    public class CreateTestQuestionCommandValidator : AbstractValidator<CreateTestQuestionCommand>
    {
        public CreateTestQuestionCommandValidator()
        {
            RuleFor(x => x.TestId)
                .GreaterThan(0)
                .WithMessage("Test ID must be greater than 0");

            RuleFor(x => x.QuestionContent)
                .NotEmpty()
                .WithMessage("Question content is required")
                .MaximumLength(1000)
                .WithMessage("Question content must not exceed 1000 characters");

            RuleFor(x => x.Score)
                .GreaterThan(0)
                .WithMessage("Score must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Score must not exceed 100");

            RuleFor(x => x.OrderNumber)
                .GreaterThan(0)
                .WithMessage("Order number must be greater than 0");

            // Validate Options for EnglishListening and Practical test types
            When(x => x.Options != null && x.Options.Any(), () =>
            {
                RuleFor(x => x.Options)
                    .Must(options => options!.Count >= 4)
                    .WithMessage("At least 4 options are required for multiple choice questions")
                    .Must(options => options!.Count <= 6)
                    .WithMessage("Maximum 6 options are allowed");

                RuleFor(x => x.Options)
                    .Must(options => options!.Count(o => o.IsCorrect) == 1)
                    .WithMessage("Exactly one option must be marked as correct");

                RuleForEach(x => x.Options)
                    .ChildRules(option =>
                    {
                        option.RuleFor(o => o.OptionContent)
                            .NotEmpty()
                            .WithMessage("Option content is required")
                            .MaximumLength(500)
                            .WithMessage("Option content must not exceed 500 characters");
                    });
            });
        }
    }
}
