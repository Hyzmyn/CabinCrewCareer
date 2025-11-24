using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.EvaluationForms.Queries.GetInterviewEvaluationFormsByActivityId
{
    public class GetInterviewEvaluationFormsByActivityIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetInterviewEvaluationFormsByActivityIdQuery, Response<List<GetInterviewEvaluationFormResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<List<GetInterviewEvaluationFormResponse>>> Handle(GetInterviewEvaluationFormsByActivityIdQuery request, CancellationToken cancellationToken)
        {
            return new Response<List<GetInterviewEvaluationFormResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<List<GetInterviewEvaluationFormResponse>>(await _unitOfWork.EvaluationForms.GetInterviewFormsAsync(request.ActivityId)),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
