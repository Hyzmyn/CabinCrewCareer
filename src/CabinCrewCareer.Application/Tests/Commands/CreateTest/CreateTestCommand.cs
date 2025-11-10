using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Tests.Commands.CreateTest
{
    public record CreateTestCommand(
        string TestName,
        string Purpose,
        TestType TestType,
        int MaxScore,
        string? AudioFileURL) : IRequest<Response<GetTestResponse>>;
}
