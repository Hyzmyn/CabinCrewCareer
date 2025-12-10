using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.RoundTypes.Commands.DeleteRoundType
{
    public class DeleteRoundTypeCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteRoundTypeCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(DeleteRoundTypeCommand request, CancellationToken cancellationToken)
        {
            var roundType = await _unitOfWork.RoundTypes.GetByIdAsync(request.RoundTypeId) ?? throw new ApiException(ResponseCode.ROUND_NOT_FOUND);

            await _unitOfWork.RoundTypes.DeleteAsync(roundType);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.DELETED_SUCCESS,
                data: ResponseCode.DELETED_SUCCESS.ToString(),
                message: ResponseCode.DELETED_SUCCESS.GetDescription());
        }
    }
}
