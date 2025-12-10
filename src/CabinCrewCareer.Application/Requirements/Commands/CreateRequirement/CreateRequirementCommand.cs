using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Requirements.Commands.CreateRequirement
{
    public record class CreateRequirementCommand(string Title, string Description, int RequirementId) : IRequest<Response<string>>;
}
