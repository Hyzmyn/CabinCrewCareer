using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.TestQuestions
{
    public class TestQuestionRepository(CabinCrewCareerDbContext context, ILogger logger) 
        : Repository<TestQuestion>(context, logger), ITestQuestionRepository
    {
    }
}
