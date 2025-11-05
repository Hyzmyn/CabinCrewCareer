using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Cities.Commands.CreateWard
{
    public class CreateWardCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateWardCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(CreateWardCommand request, CancellationToken cancellationToken)
        {
            Ward ward = new()
            {
                WardName = request.WardName,
                CityId = request.CityId
            };

            await _unitOfWork.Wards.AddAsync(ward);
            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.CREATED_SUCCESS, message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
