using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetUserByIdQuery, Response<GetUserDetailResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<GetUserDetailResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var userQuery = await _unitOfWork.Users.GetAsync(user => user.UserId == request.UserId);

            var user = userQuery
                .Include(r => r.Role)
                .Include(u => u.AirlinePartner)
                .Include(u => u.Ward)
                .FirstOrDefault() ?? throw new ApiException(ResponseCode.USER_NOT_FOUND);

            return new Response<GetUserDetailResponse>(code: (int)ResponseCode.SUCCESS, data: _mapper.Map<GetUserDetailResponse>(user), message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
