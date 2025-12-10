using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.InterviewCriterias
{
    public class InterviewCriteriaItemRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<InterviewCriteriaItem>(dbContext, logger), IInterviewCriteriaItemRepository
    {
    }
}
