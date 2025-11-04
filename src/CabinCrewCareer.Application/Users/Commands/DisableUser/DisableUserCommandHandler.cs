using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Users.Commands.DisableUser
{
    public class DisableUserCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<DisableUserCommand, Response<GetUserResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<GetUserResponse>> Handle(DisableUserCommand request, CancellationToken cancellationToken)
        {
            var userQuery = await _unitOfWork.Users.GetAsync(expression: u => u.UserId == request.UserId, disableTracking: true);

            var user = userQuery.FirstOrDefault() ?? throw new ApiException(ResponseCode.USER_NOT_FOUND);

            if (user.IsActive)
            {
                user.IsActive = false;
            }
            else
            {
                user.IsActive = true;
            }

            await _unitOfWork.Users.UpdateAsync(user);

            await _unitOfWork.CompleteAsync();

            return new Response<GetUserResponse>(code: (int)ResponseCode.DISABLED_SUCCESS, message: ResponseCode.DISABLED_SUCCESS.GetDescription());
        }
    }
}
