using CabinCrewCareer.Application.Authentication.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Security;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.Authentication.Queries.Login
{
    public class LoginQueryHandler(
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork) : IRequestHandler<LoginQuery, Response<AuthenticationResult>>
    {
        private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<AuthenticationResult>> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var userQuery = await _unitOfWork.Users.GetAsync(
                expression: user => user.UserName.ToLower().Equals(request.Username.ToLower())
                                    && user.Password != null
                                    && user.Password.Equals(request.Password),
                includeFunc: user => user.Include(u => u.Role));

            var user = userQuery.FirstOrDefault() ?? throw new ApiException(ResponseCode.INVALID_CREDENTIALS);

            if (!user.IsActive) throw new ApiException(ResponseCode.INACTIVE_USER);

            var token = _jwtTokenGenerator.GenerateJwtToken(user);
            var refreshToken = _jwtTokenGenerator.GenerateJwtRefreshToken(user);

            return new Response<AuthenticationResult>(code: (int)ResponseCode.SUCCESS, data: new AuthenticationResult(token, refreshToken), message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
