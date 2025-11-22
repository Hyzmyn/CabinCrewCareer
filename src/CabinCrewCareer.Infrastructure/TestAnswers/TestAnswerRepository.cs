using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.TestAnswers
{
    public class TestAnswerRepository(
        CabinCrewCareerDbContext dbContext,
        ILogger logger) : Repository<TestAnswer>(dbContext, logger), ITestAnswerRepository
    {
    }
}
