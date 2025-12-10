using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.RoundTypes.Commands.CreateRoundType
{
    public class CreateRoundTypeCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateRoundTypeCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(CreateRoundTypeCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.RoundTypes.AddAsync(new TypeRound
            {
                RoundTypeName = request.RoundTypeName,
                CampaignType = request.CampaignType,
            });

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.CREATED_UNSUCC,
                data: ResponseCode.CREATED_SUCCESS.ToString(),
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
