using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Locations.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Locations.Queries.GetLocationById
{
    public class GetLocationByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetLocationByIdQuery, Response<GetLocationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<GetLocationResponse>> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.Locations.GetByIdAsync(request.LocationId) ?? throw new ApiException(ResponseCode.LOCATION_NOT_FOUND);

            return new Response<GetLocationResponse>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<GetLocationResponse>(result),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
