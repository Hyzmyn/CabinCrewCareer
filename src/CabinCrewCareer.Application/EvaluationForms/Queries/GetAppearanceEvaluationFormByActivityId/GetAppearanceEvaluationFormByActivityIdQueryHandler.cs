using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.EvaluationForms.Queries.GetAppearanceEvaluationFormByActivityId
{
    public class GetAppearanceEvaluationFormByActivityIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetAppearanceEvaluationFormByActivityIdQuery, Response<GetAppearanceEvaluationFormResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<GetAppearanceEvaluationFormResponse>> Handle(GetAppearanceEvaluationFormByActivityIdQuery request, CancellationToken cancellationToken)
        {
            return new Response<GetAppearanceEvaluationFormResponse>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<GetAppearanceEvaluationFormResponse>(await _unitOfWork.EvaluationForms.GetEvaluationFormByIdAsync(EvaluationType.Appearance, request.ActivityId)),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
