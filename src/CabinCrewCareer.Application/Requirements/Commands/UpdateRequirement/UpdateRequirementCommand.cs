using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Requirements.Commands.UpdateRequirement
{
    public record UpdateRequirementCommand(int RequirementId, int RequirementItemId, string Title, string Description) : IRequest<Response<string>>;
}
