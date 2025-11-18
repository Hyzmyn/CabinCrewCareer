using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.ScoringCriterias.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.ScoringCriterias.Queries.GetScoringCriterias
{
    public class GetScoringCriteriasQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetScoringCriteriasQuery, Response<List<GetScoringCriteriaResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<List<GetScoringCriteriaResponse>>> Handle(GetScoringCriteriasQuery request, CancellationToken cancellationToken)
        {
            var criteriasQuery = await _unitOfWork.ScoringCriterias
                .GetAllAsync(
                includeFunc: s => s
                .Include(c => c.ScoringCriteriaItems)
                  .ThenInclude(c => c.ScoringCriteriaDetails));

            return new Response<List<GetScoringCriteriaResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<List<GetScoringCriteriaResponse>>(criteriasQuery.ToList()),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
