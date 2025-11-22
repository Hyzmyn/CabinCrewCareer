using FluentValidation;

namespace CabinCrewCareer.Application.TestQuestions.Queries.GetTestQuestionsForExam
{
    public class GetTestQuestionsForExamQueryValidator : AbstractValidator<GetTestQuestionsForExamQuery>
    {
        public GetTestQuestionsForExamQueryValidator()
        {
            RuleFor(x => x.TestId)
                .GreaterThan(0)
                .WithMessage("Test ID must be greater than 0");

            RuleFor(x => x.JoinCode)
                .NotEmpty()
                .WithMessage("Join code is required")
                .Length(10)
                .WithMessage("Join code must be exactly 10 characters");
        }
    }
}
