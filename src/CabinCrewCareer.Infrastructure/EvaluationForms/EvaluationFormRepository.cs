using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.EvaluationForms
{
    public class EvaluationFormRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<EvaluationForm>(dbContext, logger), IEvaluationFormRepository
    {
    }
}
