using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Requirements.Commands.DeleteRequirement
{
    public class DeleteRequirementCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteRequirementCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(DeleteRequirementCommand request, CancellationToken cancellationToken)
        {
            var roundType = await _unitOfWork.RequirementItems.GetByIdAsync(request.RequirementItemId) ?? throw new ApiException(ResponseCode.ROUND_NOT_FOUND);

            await _unitOfWork.RequirementItems.DeleteAsync(roundType);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.DELETED_SUCCESS,
                data: ResponseCode.DELETED_SUCCESS.ToString(),
                message: ResponseCode.DELETED_SUCCESS.GetDescription());
        }
    }
}
