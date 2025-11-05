using AutoMapper;
using CabinCrewCareer.Application.Cities.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Cities.Queries.GetCities
{
    public class GetCitiesQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetCitiesQuery, Response<List<GetCityResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<List<GetCityResponse>>> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
        {
            var cityQuery = await _unitOfWork.Cities.GetAllAsync();

            return new Response<List<GetCityResponse>>(code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<List<GetCityResponse>>(cityQuery.ToList()),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
