using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.TestSessions
{
    public class TestSessionRepository(
        CabinCrewCareerDbContext dbContext,
        ILogger logger) : Repository<TestSession>(dbContext, logger), ITestSessionRepository
    {
    }
}
