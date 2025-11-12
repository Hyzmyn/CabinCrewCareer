using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Roles.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Roles.Queries.GetUsersByRoleId
{
    public class GetUsersByRoleIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetUsersByRoleIdQuery, Response<List<GetRoleResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<List<GetRoleResponse>>> Handle(GetUsersByRoleIdQuery request, CancellationToken cancellationToken)
        {
            return new Response<List<GetRoleResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<List<GetRoleResponse>>(await _unitOfWork.Users.GetUsersByRoleIdAsync(request.RoleId)),
                message: ResponseCode.SUCCESS.ToString());
        }
    }
}
