using CabinCrewCareer.Application.Configurations.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Configurations.Queries.GetConfigurationById
{
    public record GetConfigurationByIdQuery(int ConfigurationId) : IRequest<Response<GetConfigurationResponse>>;
}
