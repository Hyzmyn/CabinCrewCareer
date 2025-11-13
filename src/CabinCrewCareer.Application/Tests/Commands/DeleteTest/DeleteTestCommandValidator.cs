using FluentValidation;

namespace CabinCrewCareer.Application.Tests.Commands.DeleteTest
{
    public class DeleteTestCommandValidator : AbstractValidator<DeleteTestCommand>
    {
        public DeleteTestCommandValidator()
        {
            RuleFor(x => x.TestId)
                .GreaterThan(0).WithMessage("Test ID must be greater than 0");
        }
    }
}