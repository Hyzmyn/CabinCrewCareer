using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using FluentValidation;

namespace CabinCrewCareer.Application.Users.Commands.CreateUser
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(u => u.Username)
                .NotEmpty().WithMessage("Username is required!")
                .MustAsync(async (userName, cancellation) => await CheckExistUsernameEmail(userName, "Username"))
                .WithMessage("Username has already existed!");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required!")
                .MustAsync(async (email, cancellation) => await CheckExistUsernameEmail(email, "Email"))
                .WithMessage("Email has already existed!");

            RuleFor(u => u.PhoneNumber)
                .NotEmpty().WithMessage("PhoneNumber is required!")
                .MustAsync(async (phoneNumber, cancellation) => await CheckExistUsernameEmail(phoneNumber, "PhoneNumber"))
                .WithMessage("PhoneNumber has already existed!");

            //RuleFor(p => p.RoleId)
            //    .NotEmpty().WithMessage("Role is required!")
            //    .MustAsync(async (roleId, cancellation) => await AlreadyExistId(roleId, "RoleId"))
            //    .WithMessage("Role does not exist!");            
        }

        private async Task<bool> CheckExistUsernameEmail(string value, string option)
        {
            switch (option)
            {
                case "Username":
                    var usernameQuery = await _unitOfWork.Users
                        .GetAsync(user => user.UserName.Equals(value));
                    if (usernameQuery.FirstOrDefault() is null) return true;
                    return false;
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
