using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.InterviewCriterias
{
    public class InterviewCriteriaRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<InterviewCriteria>(dbContext, logger), IInterviewCriteriaRepository
    {
    }
}
