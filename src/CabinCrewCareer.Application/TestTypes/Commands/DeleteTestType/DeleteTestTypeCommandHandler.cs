using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestTypes.Commands.DeleteTestType
{
    public class DeleteTestTypeCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteTestTypeCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(DeleteTestTypeCommand request, CancellationToken cancellationToken)
        {
            var roundType = await _unitOfWork.TestTypes.GetByIdAsync(request.TestTypeId) ?? throw new ApiException(ResponseCode.TEST_NOT_FOUND);

            await _unitOfWork.TestTypes.DeleteAsync(roundType);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.DELETED_SUCCESS,
                data: ResponseCode.DELETED_SUCCESS.ToString(),
                message: ResponseCode.DELETED_SUCCESS.GetDescription());
        }
    }
}
