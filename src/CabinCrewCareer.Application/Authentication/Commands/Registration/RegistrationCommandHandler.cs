using CabinCrewCareer.Application.Common.Constants;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Authentication.Commands.Registration
{
    public class RegistrationCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<RegistrationCommand, Response<GetUserResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<GetUserResponse>> Handle(RegistrationCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            var validator = new RegistrationCommandValidator(_unitOfWork);
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            var user = new User
            {
                UserName = request.Username,
                Password = ***REMOVED***
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Gender = (Gender)request.Gender,
                RoleId = (int)AvailableRole.Candidate,
                IsActive = true,
                ImgURL = DefaultAvatar.UserPicture,
                CreatedBy = request.Username
            };

            var newResult = await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            return new Response<GetUserResponse>(code: (int)ResponseCode.CREATED_SUCCESS, message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
