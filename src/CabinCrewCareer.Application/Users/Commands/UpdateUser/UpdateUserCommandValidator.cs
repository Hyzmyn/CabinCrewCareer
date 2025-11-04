using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Users.Common;
using FluentValidation;

namespace CabinCrewCareer.Application.Users.Commands.UpdateUser
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserRequest>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(u => u.Fullname)
                .NotEmpty().WithMessage("Fullname is required!");

            RuleFor(u => u.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required!");

            RuleFor(u => u.Address)
                .NotEmpty().WithMessage("Address is required!");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required!")
                .MustAsync(async (email, cancellation) => await CheckExistUsernameEmail(email))
                .WithMessage("Email has already existed!");
        }

        private async Task<bool> CheckExistUsernameEmail(string email)
        {
            var usernameQuery = await _unitOfWork.Users
                        .GetAsync(user => user.Email.ToLower().Equals(email.ToLower()));

            if (usernameQuery.FirstOrDefault() is null) return true;

            return false;
        }
    }
}
