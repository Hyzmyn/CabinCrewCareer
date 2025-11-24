using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface IEvaluationFormRepository : IRepository<EvaluationForm>
    {
        Task<EvaluationForm> GetEvaluationFormByIdAsync(EvaluationType type, int activityId);
    }
}
