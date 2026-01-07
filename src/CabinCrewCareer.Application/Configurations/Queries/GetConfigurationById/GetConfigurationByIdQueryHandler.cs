using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Configurations.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Configurations.Queries.GetConfigurationById
{
    public class GetConfigurationByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetConfigurationByIdQuery, Response<GetConfigurationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<GetConfigurationResponse>> Handle(GetConfigurationByIdQuery request, CancellationToken cancellationToken)
        {
            var userQuery = await _unitOfWork.RoundConfigurations.GetAsync(user => user.RoundConfigurationId == request.ConfigurationId);

            var user = userQuery
                .FirstOrDefault() ?? throw new ApiException(ResponseCode.USER_NOT_FOUND);

            return new Response<GetConfigurationResponse>(code: (int)ResponseCode.SUCCESS, data: _mapper.Map<GetConfigurationResponse>(user), message: ResponseCode.SUCCESS.ToString());
        }
    }
}
