using FluentValidation;

namespace CabinCrewCareer.Application.TestQuestions.Commands.CreateBulkTestQuestions
{
    public class CreateBulkTestQuestionsCommandValidator : AbstractValidator<CreateBulkTestQuestionsCommand>
    {
        public CreateBulkTestQuestionsCommandValidator()
        {
            RuleFor(x => x.TestId)
                .GreaterThan(0)
                .WithMessage("Test ID must be greater than 0");

            RuleFor(x => x.Questions)
                .NotEmpty()
                .WithMessage("At least one question is required")
                .Must(questions => questions.Count <= 50)
                .WithMessage("Maximum 50 questions can be created at once");

            RuleForEach(x => x.Questions)
                .ChildRules(question =>
                {
                    question.RuleFor(q => q.QuestionContent)
                        .NotEmpty()
                        .WithMessage("Question content is required")
                        .MaximumLength(1000)
                        .WithMessage("Question content must not exceed 1000 characters");

                    question.RuleFor(q => q.Score)
                        .GreaterThan(0)
                        .WithMessage("Score must be greater than 0")
                        .LessThanOrEqualTo(100)
                        .WithMessage("Score must not exceed 100");

                    question.RuleFor(q => q.OrderNumber)
                        .GreaterThan(0)
                        .WithMessage("Order number must be greater than 0");

                    // Validate Options when provided
                    question.When(q => q.Options != null && q.Options.Any(), () =>
                    {
                        question.RuleFor(q => q.Options)
                            .Must(options => options!.Count >= 4)
                            .WithMessage("At least 4 options are required for multiple choice questions")
                            .Must(options => options!.Count <= 6)
                            .WithMessage("Maximum 6 options are allowed");

                        question.RuleFor(q => q.Options)
                            .Must(options => options!.Count(o => o.IsCorrect) == 1)
                            .WithMessage("Exactly one option must be marked as correct");

                        question.RuleForEach(q => q.Options)
                            .ChildRules(option =>
                            {
                                option.RuleFor(o => o.OptionContent)
                                    .NotEmpty()
                                    .WithMessage("Option content is required")
                                    .MaximumLength(500)
                                    .WithMessage("Option content must not exceed 500 characters");
                            });
                    });
                });
        }
    }
}
