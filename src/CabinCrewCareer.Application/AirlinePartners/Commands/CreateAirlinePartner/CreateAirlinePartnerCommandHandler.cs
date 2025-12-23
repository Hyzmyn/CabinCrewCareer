using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.AirlinePartners.Commands.CreateAirlinePartner
{
    public class CreateAirlinePartnerCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<CreateAirlinePartnerCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(CreateAirlinePartnerCommand request, CancellationToken cancellationToken)
        {
            AirlinePartner ward = new()
            {
                PartnerName = request.PartnerName,
                Description = request.Description,
                Address = request.Address,
                WardId = request.WardId,
            };

            await _unitOfWork.AirlinePartners.AddAsync(ward);
            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.CREATED_SUCCESS, data: ResponseCode.CREATED_SUCCESS.ToString(), message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
