using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.RoundTypes.Commands.UpdateRoundType
{
    public class UpdateRoundTypeCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateRoundTypeCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(UpdateRoundTypeCommand request, CancellationToken cancellationToken)
        {
            var roundType = await _unitOfWork.RoundTypes.GetByIdAsync(request.RoundTypeId) ?? throw new ApiException(ResponseCode.ROUND_NOT_FOUND);

            roundType.RoundTypeName = request.RoundTypeName;

            await _unitOfWork.RoundTypes.UpdateAsync(roundType);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.UPDATED_SUCCESS,
                data: ResponseCode.UPDATED_SUCCESS.ToString(),
                message: ResponseCode.UPDATED_SUCCESS.GetDescription());
        }
    }
}
