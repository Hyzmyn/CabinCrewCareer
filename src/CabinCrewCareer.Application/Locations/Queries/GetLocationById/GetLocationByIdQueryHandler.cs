using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Locations.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.Locations.Queries.GetLocationById
{
    public class GetLocationByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetLocationByIdQuery, Response<GetLocationDetailResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<GetLocationDetailResponse>> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
        {
            var temp = await _unitOfWork.Locations.GetAsync(
                expression: x => x.LocationId == request.LocationId,
                includeFunc: x => x.Include(x => x.Ward).ThenInclude(x => x.City));

            var result = await temp.FirstOrDefaultAsync(cancellationToken: cancellationToken) ?? throw new ApiException(ResponseCode.LOCATION_NOT_FOUND);

            return new Response<GetLocationDetailResponse>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<GetLocationDetailResponse>(result),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
