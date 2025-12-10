using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.InterviewCriterias.Commands.DeleteInterviewCriteria
{
    public class DeleteInterviewCriteriaCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteInterviewCriteriaCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(DeleteInterviewCriteriaCommand request, CancellationToken cancellationToken)
        {
            var roundType = await _unitOfWork.InterviewCriteriaItems.GetByIdAsync(request.InterviewCriteriaItemId) ?? throw new ApiException(ResponseCode.ROUND_NOT_FOUND);

            await _unitOfWork.InterviewCriteriaItems.DeleteAsync(roundType);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.DELETED_SUCCESS,
                data: ResponseCode.DELETED_SUCCESS.ToString(),
                message: ResponseCode.DELETED_SUCCESS.GetDescription());
        }
    }
}
