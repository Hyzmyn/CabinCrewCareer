using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestTypes.Commands.UpdateTestType
{
    public record UpdateTestTypeCommand(int TestTypeId, string TestTypeName) : IRequest<Response<string>>;
}
