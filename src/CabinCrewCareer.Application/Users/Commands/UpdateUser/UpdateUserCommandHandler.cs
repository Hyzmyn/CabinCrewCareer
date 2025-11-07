using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            var userQuery = await _unitOfWork.Users.GetAsync(expression: u => u.UserId == request.Id, disableTracking: true);

            var user = userQuery.FirstOrDefault();

            if (user is null)
            {
                errorMessages.Add(ResponseCode.USER_NOT_FOUND.GetDescription());
                throw new ValidationException(ResponseCode.USER_NOT_FOUND, errorMessages);
            }

            var validator = new UpdateUserCommandValidator(_unitOfWork);
            var result = await validator.ValidateAsync(request.UpdateUserRequest, cancellationToken);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            user.FullName = request.UpdateUserRequest.Fullname;
            user.Email = request.UpdateUserRequest.Email;
            user.PhoneNumber = request.UpdateUserRequest.PhoneNumber;
            user.Address = request.UpdateUserRequest.Address;
            user.DateOfBirth = request.UpdateUserRequest.DateOfBirth;
            user.Gender = request.UpdateUserRequest.Gender;
            user.WardId = request.UpdateUserRequest.WardId;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.UPDATED_SUCCESS, data: ResponseCode.UPDATED_SUCCESS.ToString(), message: ResponseCode.UPDATED_SUCCESS.GetDescription());
        }
    }
}
