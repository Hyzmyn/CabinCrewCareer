using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using FluentValidation;

namespace CabinCrewCareer.Application.Authentication.Commands.Registration
{
    public class RegistrationCommandValidator : AbstractValidator<RegistrationCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegistrationCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(u => u.Username)
                .NotEmpty().WithMessage("Username is required!")
                .MustAsync(async (userName, cancellation) => await CheckExistUsernameEmail(userName, "Username"))
                .WithMessage("Username has already existed!");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("Password is required!");

            RuleFor(u => u.ConfirmedPassword)
                .NotEmpty().WithMessage("Confirmed Password is required!")
                .Equal(u => u.Password).WithMessage("Password must match");

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

        private async Task<bool> AlreadyExistId(int id, string option)
        {
            switch (option.ToLower())
            {
                //case "roleid":
                //    var role = await _unitOfWork.Roles.GetByIdAsync(id);
                //    if (role is null) return false;
                //    return true;
                //case "schoolid":
                //    var school = await _unitOfWork.Schools.GetByIdAsync(id);
                //    if (school is null) return false;
                //    return true;
                //case "gradeid":
                //    var grade = await _unitOfWork.Grades.GetByIdAsync(id);
                //    if (grade is null) return false;
                //    return true;
                default:
                    return false;
            }
        }

        private async Task<bool> CheckExistUsernameEmail(string value, string option)
        {
            switch (option)
            {
                case "Username":
                    var usernameQuery = await _unitOfWork.Users
                        .GetAsync(user => user.UserName.ToLower().Equals(value.ToLower()));
                    if (usernameQuery.FirstOrDefault() is null) return true;
                    return false;
                case "Email":
                    var emailQuery = await _unitOfWork.Users
                        .GetAsync(user => user.Email.ToLower().Equals(value.ToLower()));
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
