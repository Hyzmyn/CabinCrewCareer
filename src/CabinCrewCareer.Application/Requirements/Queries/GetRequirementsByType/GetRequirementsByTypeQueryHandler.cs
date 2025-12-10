using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Requirements.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.Requirements.Queries.GetRequirementsByType
{
    public class GetRequirementsByTypeQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetRequirementsByTypeQuery, Response<GetRequirementResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<GetRequirementResponse>> Handle(GetRequirementsByTypeQuery request, CancellationToken cancellationToken)
        {
            var roundTypesQuery = await _unitOfWork.Requirements.GetAllAsync(expression: x => x.CampaignType == request.CampaignType,
                includeFunc: x => x.Include(x => x.RequirementItems));

            var roundTypes = await roundTypesQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken) ?? throw new ApiException(ResponseCode.ROUND_NOT_FOUND);

            GetRequirementResponse type = new()
            {
                RequirementId = roundTypes.RequirementId,
                RequirementItems = [.. roundTypes.RequirementItems.Select(x => new GetRequirementItemResponse
                {
                    RequirementItemId = x.RequirementItemId,
                    Title = x.Title,
                    Description = x.Description
                })]
            };

            return new Response<GetRequirementResponse>(
                code: (int)ResponseCode.SUCCESS,
                data: type,
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
