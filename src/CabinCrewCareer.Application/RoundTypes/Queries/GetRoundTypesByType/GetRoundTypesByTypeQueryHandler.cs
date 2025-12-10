using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.RoundTypes.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.RoundTypes.Queries.GetRoundTypesByType
{
    public class GetRoundTypesByTypeQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetRoundTypesByTypeQuery, Response<List<GetRoundTypeResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<List<GetRoundTypeResponse>>> Handle(GetRoundTypesByTypeQuery request, CancellationToken cancellationToken)
        {
            var roundTypesQuery = await _unitOfWork.RoundTypes.GetAllAsync(expression: x => x.CampaignType == request.CampaignType);

            var roundTypes = await roundTypesQuery.ToListAsync(cancellationToken: cancellationToken);

            List<GetRoundTypeResponse> types = [];

            foreach (var item in roundTypes)
            {
                types.Add(new GetRoundTypeResponse
                {
                    RoundTypeId = item.RoundTypeId,
                    RoundTypeName = item.RoundTypeName
                });
            }

            return new Response<List<GetRoundTypeResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: types,
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
