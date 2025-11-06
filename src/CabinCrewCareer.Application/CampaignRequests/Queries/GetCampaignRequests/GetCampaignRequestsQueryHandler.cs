using AutoMapper;
using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignRequests.Queries.GetCampaignRequests
{
    public class GetCampaignRequestsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetCampaignRequestsQuery, Response<PaginatedList<GetCampaignRequestResponse>>>
    {
        public async Task<Response<PaginatedList<GetCampaignRequestResponse>>> Handle(
            GetCampaignRequestsQuery request,
            CancellationToken cancellationToken)
        {
            var campaignRequests = await unitOfWork.CampaignRequests.PaginatedListAsync(
                request.SearchTerm,
                request.SortColumn,
                request.SortOrder,
                request.Status,
                request.RequestType,
                request.PartnerId,
                request.Page,
                request.PageSize);

            var mappedRequests = mapper.Map<PaginatedList<GetCampaignRequestResponse>>(campaignRequests);

            return new Response<PaginatedList<GetCampaignRequestResponse>>(code: (int)ResponseCode.SUCCESS, data: mappedRequests, message: ResponseCode.SUCCESS.ToString());
        }
    }
}
