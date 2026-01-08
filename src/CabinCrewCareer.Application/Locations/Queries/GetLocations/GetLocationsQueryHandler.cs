using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Locations.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.Locations.Queries.GetLocations
{
    public class GetLocationsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetLocationsQuery, Response<List<GetLocationResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<List<GetLocationResponse>>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
        {
            var cityQuery = await _unitOfWork.Locations.GetAllAsync(includeFunc: x => x.Include(x => x.Ward).ThenInclude(x => x.City));

            return new Response<List<GetLocationResponse>>(code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<List<GetLocationResponse>>(cityQuery.ToList()),
                message: ResponseCode.SUCCESS.ToString());
        }
    }
}
