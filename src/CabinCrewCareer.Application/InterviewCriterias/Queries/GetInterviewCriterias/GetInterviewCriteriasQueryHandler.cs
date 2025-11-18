using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.InterviewCriterias.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.InterviewCriterias.Queries.GetInterviewCriterias
{
    public class GetInterviewCriteriasQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetInterviewCriteriasQuery, Response<List<GetInterviewCriteriaResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<List<GetInterviewCriteriaResponse>>> Handle(GetInterviewCriteriasQuery request, CancellationToken cancellationToken)
        {
            var criteriasQuery = await _unitOfWork.InterviewCriterias
                .GetAllAsync(
                includeFunc: s => s
                .Include(c => c.InterviewCriteriaItems));

            return new Response<List<GetInterviewCriteriaResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<List<GetInterviewCriteriaResponse>>(criteriasQuery.ToList()),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
