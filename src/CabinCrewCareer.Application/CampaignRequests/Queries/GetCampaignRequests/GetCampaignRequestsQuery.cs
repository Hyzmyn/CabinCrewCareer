using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignRequests.Queries.GetCampaignRequests
{
    public class GetCampaignRequestsQuery : IRequest<Response<PaginatedList<GetCampaignRequestResponse>>>
    {
        public string? SearchTerm { get; set; }
        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; }
        public int? Status { get; set; }
        public int? RequestType { get; set; }
        public int? PartnerId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
