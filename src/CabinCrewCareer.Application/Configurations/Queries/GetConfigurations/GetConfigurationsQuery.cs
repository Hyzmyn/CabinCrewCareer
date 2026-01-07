using CabinCrewCareer.Application.Configurations.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Configurations.Queries.GetConfigurations
{
    public record GetConfigurationsQuery() : IRequest<Response<List<GetConfigurationResponse>>>;
}
