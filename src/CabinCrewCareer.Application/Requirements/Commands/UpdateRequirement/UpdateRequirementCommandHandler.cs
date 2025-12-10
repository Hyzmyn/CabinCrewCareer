using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Requirements.Commands.UpdateRequirement
{
    public class UpdateRequirementCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateRequirementCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(UpdateRequirementCommand request, CancellationToken cancellationToken)
        {
            var roundType = await _unitOfWork.RequirementItems.GetByIdAsync(request.RequirementItemId) ?? throw new ApiException(ResponseCode.ROUND_NOT_FOUND);

            roundType.Title = request.Title;
            roundType.Description = request.Description;

            await _unitOfWork.RequirementItems.UpdateAsync(roundType);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.UPDATED_SUCCESS,
                data: ResponseCode.UPDATED_SUCCESS.ToString(),
                message: ResponseCode.UPDATED_SUCCESS.GetDescription());
        }
    }
}
