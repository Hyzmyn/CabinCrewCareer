using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Requirements.Commands.DeleteRequirement
{
    public record DeleteRequirementCommand(int RequirementId, int RequirementItemId) : IRequest<Response<string>>;
}
