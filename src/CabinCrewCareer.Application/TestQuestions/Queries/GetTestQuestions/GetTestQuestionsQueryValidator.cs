using FluentValidation;

namespace CabinCrewCareer.Application.TestQuestions.Queries.GetTestQuestions
{
    public class GetTestQuestionsQueryValidator : AbstractValidator<GetTestQuestionsQuery>
    {
        public GetTestQuestionsQueryValidator()
        {
            RuleFor(x => x.TestId)
                .GreaterThan(0)
                .WithMessage("Test ID must be greater than 0");
        }
    }
}
