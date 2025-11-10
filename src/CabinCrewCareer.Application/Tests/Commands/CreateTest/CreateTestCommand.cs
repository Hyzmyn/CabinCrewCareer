using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CabinCrewCareer.Application.Tests.Commands.CreateTest
{
    public record CreateTestCommand(
        string TestName,
        string Purpose,
        TestType TestType,
        int MaxScore,
        IFormFile? AudioFile) : IRequest<Response<GetTestResponse>>;
}
