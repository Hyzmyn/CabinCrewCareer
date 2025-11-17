using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.TestOptions
{
    public class TestOptionRepository(CabinCrewCareerDbContext context, ILogger logger) 
        : Repository<TestOption>(context, logger), ITestOptionRepository
    {
    }
}
