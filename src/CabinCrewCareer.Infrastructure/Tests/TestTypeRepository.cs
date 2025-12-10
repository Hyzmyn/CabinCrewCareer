using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Tests
{
    public class TestTypeRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<TypeTest>(dbContext, logger), ITestTypeRepository
    {
    }
}
