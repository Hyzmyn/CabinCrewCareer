using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestTypes.Commands.UpdateTestType
{
    public class UpdateTestTypeCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateTestTypeCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(UpdateTestTypeCommand request, CancellationToken cancellationToken)
        {
            var roundType = await _unitOfWork.TestTypes.GetByIdAsync(request.TestTypeId) ?? throw new ApiException(ResponseCode.TEST_NOT_FOUND);

            roundType.TestTypeName = request.TestTypeName;

            await _unitOfWork.TestTypes.UpdateAsync(roundType);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.UPDATED_SUCCESS,
                data: ResponseCode.UPDATED_SUCCESS.ToString(),
                message: ResponseCode.UPDATED_SUCCESS.GetDescription());
        }
    }
}
