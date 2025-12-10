using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.InterviewCriterias.Commands.UpdateInterviewCriteria
{
    public class UpdateInterviewCriteriaCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateInterviewCriteriaCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(UpdateInterviewCriteriaCommand request, CancellationToken cancellationToken)
        {
            var roundType = await _unitOfWork.InterviewCriteriaItems.GetByIdAsync(request.InterviewCriteriaItemId) ?? throw new ApiException(ResponseCode.ROUND_NOT_FOUND);

            roundType.Criteria = request.Criteria;

            await _unitOfWork.InterviewCriteriaItems.UpdateAsync(roundType);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.UPDATED_SUCCESS,
                data: ResponseCode.UPDATED_SUCCESS.ToString(),
                message: ResponseCode.UPDATED_SUCCESS.GetDescription());
        }
    }
}
