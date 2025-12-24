using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Queries.GetCampaignList
{
    public record GetCampaignListQuery(
        string? SearchTerm,
        string? SortColumn,
        string? SortOrder,
        int? CampaignType,
        int? CampaignStatus,
        int? PartnerId,
        int Page,
        int PageSize) : IRequest<Response<PaginatedList<GetCampaignListDto>>>;
}
