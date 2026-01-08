using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.Configurations.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Configurations.Queries.GetConfigurations
{
    public class GetConfigurationsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetConfigurationsQuery, Response<PaginatedList<GetConfigurationResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<PaginatedList<GetConfigurationResponse>>> Handle(GetConfigurationsQuery request, CancellationToken cancellationToken)
        {
            return new Response<PaginatedList<GetConfigurationResponse>>(code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<PaginatedList<GetConfigurationResponse>>(await _unitOfWork.RoundConfigurations.PaginatedListAsync(
                    request.SortColumn,
                    request.SortOrder,
                    request.CampaignType,
                    request.RoundTypeId,
                    request.Page,
                    request.PageSize
                )),
                message: ResponseCode.SUCCESS.ToString());
        }
    }
}
