using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CabinCrewCareer.Application.Tests.Commands.UpdateTest
{
    public record UpdateTestCommand(
        int TestId,
        string TestName,
        string Purpose,
        TestType TestType,
        int MaxScore,
        int DurationInMinutes,
        IFormFile? AudioFile,
        bool RemoveAudio = false) : IRequest<Response<GetTestResponse>>;
}