using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.EvaluationForms
{
    public class EvaluationFormRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<EvaluationForm>(dbContext, logger), IEvaluationFormRepository
    {
        public async Task<EvaluationForm> GetEvaluationFormByIdAsync(EvaluationType type, int activityId)
        {
            IQueryable<EvaluationForm> query = _dbContext.EvaluationForms
                .Select(
                x => new EvaluationForm
                {
                    EvaluationId = x.EvaluationId,
                    EvaluatedDate = x.EvaluatedDate,
                    IsPassed = x.IsPassed,
                    RoundActivity = new RoundActivity
                    {
                        Round = new Round
                        {
                            RoundType = x.RoundActivity.Round.RoundType
                        },
                        User = new User
                        {
                            FullName = x.RoundActivity.User.FullName
                        }
                    },
                    Examiner = new User
                    {
                        FullName = x.Examiner.FullName
                    }
                });

            if (type == EvaluationType.Appearance)
            {
                query = query.Where(x => x.ActivityId == activityId).Include(x => x.AppearanceResults);
            }
            else
            {
                query = query.Where(x => x.ActivityId == activityId).Include(x => x.InterviewResults);
            }

            var result = await query.FirstOrDefaultAsync();

            if (result is null) return new EvaluationForm();

            return result;
        }
    }
}
