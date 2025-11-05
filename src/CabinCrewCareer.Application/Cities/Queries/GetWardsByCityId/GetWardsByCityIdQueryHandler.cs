using AutoMapper;
using CabinCrewCareer.Application.Cities.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Cities.Queries.GetWardsByCityId
{
    public class GetWardsByCityIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetWardsByCityIdQuery, Response<List<GetWardResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<List<GetWardResponse>>> Handle(GetWardsByCityIdQuery request, CancellationToken cancellationToken)
        {
            var cityQuery = await _unitOfWork.Wards.GetAllAsync(
                expression: ward => ward.CityId == request.CityId);

            return new Response<List<GetWardResponse>>(code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<List<GetWardResponse>>(cityQuery.ToList()),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
