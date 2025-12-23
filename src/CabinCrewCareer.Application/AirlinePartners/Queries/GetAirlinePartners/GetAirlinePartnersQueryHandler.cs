using AutoMapper;
using CabinCrewCareer.Application.AirlinePartners.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.AirlinePartners.Queries.GetAirlinePartners
{
    public class GetAirlinePartnersQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetAirlinePartnersQuery, Response<List<GetAirlinePartnerResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<List<GetAirlinePartnerResponse>>> Handle(GetAirlinePartnersQuery request, CancellationToken cancellationToken)
        {
            var cityQuery = await _unitOfWork.AirlinePartners.GetAllAsync(includeFunc: x => x.Include(x => x.Ward).ThenInclude(x => x.City));

            return new Response<List<GetAirlinePartnerResponse>>(code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<List<GetAirlinePartnerResponse>>(cityQuery.ToList()),
                message: ResponseCode.SUCCESS.ToString());
        }
    }
}
