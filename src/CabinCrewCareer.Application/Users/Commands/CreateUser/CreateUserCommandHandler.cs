using CabinCrewCareer.Application.Common.Constants;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Common.Models.Requests;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using System.Text;

namespace CabinCrewCareer.Application.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService) : IRequestHandler<CreateUserCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IEmailService _emailService = emailService;
        private static readonly Random random = new();

        public async Task<Response<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var mailRequest = new MailRequest();
            List<string> errorMessages = [];

            var validator = new CreateUserCommandValidator(_unitOfWork);
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            var user = new User
            {
                UserName = request.Username,
                Password = ***REMOVED***
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Gender = Gender.Others,
                IsActive = true,
                ImgURL = DefaultAvatar.UserPicture,
                RoleId = request.RoleId switch
                {
                    (int)AvailableRole.Recruiter => (int)AvailableRole.Recruiter,
                    (int)AvailableRole.Examiner => (int)AvailableRole.Examiner,
                    _ => (int?)(int)AvailableRole.AirlinePartner,
                }
            };

            var newResult = await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            mailRequest.ToEmail = user.Email;
            mailRequest.Subject = "TÀI KHOẢN ĐĂNG NHẬP PHẦN MỀM";

            // Đọc template email và thay thế các tham số động
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "EmailTemplate.html");
            string templateBody = await File.ReadAllTextAsync(templatePath, cancellationToken);
            templateBody = templateBody.Replace("{FullName}", user.FullName ?? "")
                                     .Replace("{UserName}", user.UserName ?? "")
                                     .Replace("{Password}", user.Password ?? "");
            mailRequest.Body = templateBody;

            await _emailService.SendEmailAsync(mailRequest);

            return new Response<string>(code: (int)ResponseCode.CREATED_SUCCESS, data: ResponseCode.CREATED_SUCCESS.ToString(), message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder stringBuilder = new(length);

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(chars[random.Next(chars.Length)]);
            }

            return stringBuilder.ToString();
        }
    }
}
