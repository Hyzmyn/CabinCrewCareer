using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence
{
    public interface IEvaluationFormRepository : IRepository<EvaluationForm>
    {
        Task<EvaluationForm> GetEvaluationFormByIdAsync(int activityId);

        Task<EvaluationForm> GetInterviewFormByIdAsync(int evaluationId);

        Task<List<EvaluationForm>> GetInterviewFormsAsync(int activityId);
    }
}
