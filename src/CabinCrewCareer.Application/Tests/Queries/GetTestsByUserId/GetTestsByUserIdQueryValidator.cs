using FluentValidation;

namespace CabinCrewCareer.Application.Tests.Queries.GetTestsByUserId
{
    public class GetTestsByUserIdQueryValidator : AbstractValidator<GetTestsByUserIdQuery>
    {
        public GetTestsByUserIdQueryValidator()
        {
            // No validation needed - userId comes from token
        }
    }
}
