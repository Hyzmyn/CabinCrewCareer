using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.EvaluationForms.Queries.GetInterviewEvaluationFormByActivityId
{
    public class GetInterviewEvaluationFormByActivityIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetInterviewEvaluationFormByActivityIdQuery, Response<GetInterviewEvaluationFormResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<GetInterviewEvaluationFormResponse>> Handle(GetInterviewEvaluationFormByActivityIdQuery request, CancellationToken cancellationToken)
        {
            return new Response<GetInterviewEvaluationFormResponse>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<GetInterviewEvaluationFormResponse>(await _unitOfWork.EvaluationForms.GetEvaluationFormByIdAsync(EvaluationType.Interview, request.ActivityId)),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
