using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.Configurations.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Configurations.Queries.GetConfigurations
{
    public record GetConfigurationsQuery(
        string? SortColumn,
        string? SortOrder,
        int? CampaignType,
        int? RoundTypeId,
        int Page,
        int PageSize) : IRequest<Response<PaginatedList<GetConfigurationResponse>>>;
}
