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

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required!")
                .MustAsync(async (email, cancellation) => await CheckExistUsernameEmail(email, "Email"))
                .WithMessage("Email has already existed!");

            RuleFor(u => u.PhoneNumber)
                .NotEmpty().WithMessage("PhoneNumber is required!")
                .MustAsync(async (phoneNumber, cancellation) => await CheckExistUsernameEmail(phoneNumber, "PhoneNumber"))
                .WithMessage("PhoneNumber has already existed!");

            //RuleFor(u => u.Address)
            //    .NotEmpty().WithMessage("Address is required!");
        }

        private async Task<bool> CheckExistUsernameEmail(string value, string option)
        {
            switch (option)
            {
                case "Email":
                    var emailQuery = await _unitOfWork.Users
                        .GetAsync(user => user.Email.Equals(value));
                    if (emailQuery.FirstOrDefault() is null) return true;
                    return false;
                case "PhoneNumber":
                    var phoneNumberQuery = await _unitOfWork.Users
                        .GetAsync(user => user.PhoneNumber.Equals(value));
                    if (phoneNumberQuery.FirstOrDefault() is null) return true;
                    return false;
                default:
                    return false;
            }
        }
    }
}
