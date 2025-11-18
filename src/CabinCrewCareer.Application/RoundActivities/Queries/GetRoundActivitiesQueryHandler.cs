using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.RoundActivities.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.RoundActivities.Queries
{
    public class GetRoundActivitiesQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetRoundActivitiesQuery, Response<PaginatedList<GetActivityResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<PaginatedList<GetActivityResponse>>> Handle(GetRoundActivitiesQuery request, CancellationToken cancellationToken)
        {
            return new Response<PaginatedList<GetActivityResponse>>(code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<PaginatedList<GetActivityResponse>>(await _unitOfWork.RoundActivites.PaginatedListAsync(
                    request.SearchTerm,
                    request.SortColumn,
                    request.SortOrder,
                    request.Status,
                    request.RoundId,
                    request.Page,
                    request.PageSize
                )),
                message: ResponseCode.SUCCESS.ToString());
        }
    }
}
