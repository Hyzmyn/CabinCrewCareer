using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Configurations.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Configurations.Queries.GetConfigurations
{
    public class GetConfigurationsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetConfigurationsQuery, Response<List<GetConfigurationResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<List<GetConfigurationResponse>>> Handle(GetConfigurationsQuery request, CancellationToken cancellationToken)
        {
            var cityQuery = await _unitOfWork.RoundConfigurations.GetAllAsync();

            return new Response<List<GetConfigurationResponse>>(code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<List<GetConfigurationResponse>>(cityQuery.ToList()),
                message: ResponseCode.SUCCESS.ToString());
        }
    }
}
