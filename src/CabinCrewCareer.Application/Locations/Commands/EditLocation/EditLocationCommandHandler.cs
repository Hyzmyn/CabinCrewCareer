using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Locations.Commands.EditLocation
{
    public class EditLocationCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<EditLocationCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(EditLocationCommand request, CancellationToken cancellationToken)
        {
            var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId) ?? throw new ApiException(ResponseCode.LOCATION_NOT_FOUND);

            location.Address = request.EditLocationRequest.Address;
            location.WardId = request.EditLocationRequest.WardId;

            await _unitOfWork.Locations.UpdateAsync(location);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(
                code: (int)ResponseCode.UPDATED_SUCCESS,
                data: ResponseCode.UPDATED_SUCCESS.ToString(),
                message: ResponseCode.UPDATED_SUCCESS.GetDescription());
        }
    }
}
