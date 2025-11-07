using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Cities.Commands.UpdateWard
{
    public class UpdateWardCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateWardCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(UpdateWardCommand request, CancellationToken cancellationToken)
        {
            var wardQuery = await _unitOfWork.Wards.GetAsync(expression: u => u.WardId == request.WardId, disableTracking: true);

            var ward = wardQuery.FirstOrDefault() ?? throw new ApiException(ResponseCode.WARD_NOT_FOUND);

            ward.WardName = request.WardName;

            await _unitOfWork.Wards.UpdateAsync(ward);
            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.UPDATED_SUCCESS, data: ResponseCode.UPDATED_SUCCESS.ToString(), message: ResponseCode.UPDATED_SUCCESS.GetDescription());
        }
    }
}
