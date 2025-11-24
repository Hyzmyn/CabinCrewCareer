using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.EvaluationForms.Queries.GetInterviewEvaluationFormById
{
    public class GetInterviewEvaluationFormByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetInterviewEvaluationFormByIdQuery, Response<GetInterviewEvaluationFormDetailResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<GetInterviewEvaluationFormDetailResponse>> Handle(GetInterviewEvaluationFormByIdQuery request, CancellationToken cancellationToken)
        {
            return new Response<GetInterviewEvaluationFormDetailResponse>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<GetInterviewEvaluationFormDetailResponse>(await _unitOfWork.EvaluationForms.GetInterviewFormByIdAsync(request.Id)),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
