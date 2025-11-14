using FluentValidation;

namespace CabinCrewCareer.Application.Tests.Queries.GetTestById
{
    public class GetTestByIdQueryValidator : AbstractValidator<GetTestByIdQuery>
    {
        public GetTestByIdQueryValidator()
        {
            RuleFor(x => x.TestId)
                .GreaterThan(0)
                .WithMessage("Test ID must be greater than 0");
        }
    }
}
