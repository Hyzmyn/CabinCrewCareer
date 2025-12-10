using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.ScoringCriterias.Commands.DeleteScoringCriteria
{
    public class DeleteScoringCriteriaCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteScoringCriteriaCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(DeleteScoringCriteriaCommand request, CancellationToken cancellationToken)
        {
            var roundTypeQuery = await _unitOfWork.ScoringCriteriaItems.GetAsync(expression: x => x.ScoringCriteriaItemId == request.ScoringCriteriaItemId,
                includeFunc: x => x.Include(x => x.ScoringCriteriaDetails));

            var roundType = await roundTypeQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken) ?? throw new ApiException(ResponseCode.ROUND_NOT_FOUND);

            await _unitOfWork.ScoringCriteriaDetails.DeleteRangeAsync(roundType.ScoringCriteriaDetails);

            await _unitOfWork.CompleteAsync();

            await _unitOfWork.ScoringCriteriaItems.DeleteAsync(roundType);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.DELETED_SUCCESS,
                data: ResponseCode.DELETED_SUCCESS.ToString(),
                message: ResponseCode.DELETED_SUCCESS.GetDescription());
        }
    }
}
