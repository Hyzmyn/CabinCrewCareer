using CabinCrewCareer.Application.Common.Constants;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Common.Models.Requests;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using System.Text;

namespace CabinCrewCareer.Application.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService) : IRequestHandler<CreateUserCommand, Response<GetUserResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IEmailService _emailService = emailService;
        private static readonly Random random = new();

        public async Task<Response<GetUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
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
            mailRequest.Body = "Xin chào: " + $"{user.FullName}" + ",\r\n\r\nHệ thống gửi tài khoản để  đăng nhập phần mềm:\r\n\r\nUsername\r\n\r\n" + $"{user.UserName}" + "\r\n\r\nHãy giữ kín việc này\r\n\r\nPassword\r\n\r\n" + $"{user.Password}" + "\r\n\r\nNhập đúng ký tự hoa/thường\r\n\r\nTrân trọng,\r\n\r\nHệ thống Cabin Crew Career";

            await _emailService.SendEmailAsync(mailRequest);

            return new Response<GetUserResponse>(code: (int)ResponseCode.CREATED_SUCCESS, message: ResponseCode.CREATED_SUCCESS.GetDescription());
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
