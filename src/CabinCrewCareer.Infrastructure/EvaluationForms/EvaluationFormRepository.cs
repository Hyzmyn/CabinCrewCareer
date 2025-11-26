using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.EvaluationForms
{
    public class EvaluationFormRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<EvaluationForm>(dbContext, logger), IEvaluationFormRepository
    {
        public async Task<EvaluationForm> GetEvaluationFormByIdAsync(int activityId)
        {
            IQueryable<EvaluationForm> query = _dbContext.EvaluationForms;

            query = query.Where(x => x.ActivityId == activityId).Select(x => new EvaluationForm
            {
                EvaluationId = x.EvaluationId,
                EvaluatedDate = x.EvaluatedDate,
                Comment = x.Comment,
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
                },
                AppearanceResults = x.AppearanceResults.Select(u => new AppearanceResult
                {
                    IsPassed = u.IsPassed,
                    ScoringCriteriaItemId = u.ScoringCriteriaItemId
                }).ToList()
            });

            var result = await query.FirstOrDefaultAsync();

            if (result is null) return new EvaluationForm();

            return result;
        }

        public async Task<EvaluationForm> GetInterviewFormByIdAsync(int evaluationId)
        {
            IQueryable<EvaluationForm> query = _dbContext.EvaluationForms;

            query = query.Where(x => x.EvaluationId == evaluationId).Select(x => new EvaluationForm
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
                },
                InterviewResults = x.InterviewResults.Select(u => new InterviewResult
                {
                    Score = u.Score,
                    InterviewCriteriaItemId = u.InterviewCriteriaItemId,
                    Comment = u.Comment
                }).ToList()
            });

            var result = await query.FirstOrDefaultAsync();

            if (result is null) return new EvaluationForm();

            return result;
        }

        public async Task<List<EvaluationForm>> GetInterviewFormsAsync(int activityId)
        {
            IQueryable<EvaluationForm> query = _dbContext.EvaluationForms;

            query = query.Where(x => x.ActivityId == activityId).Select(x => new EvaluationForm
            {
                EvaluationId = x.EvaluationId,
                EvaluatedDate = x.EvaluatedDate,
                Comment = x.Comment,
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
                },
                InterviewResults = x.InterviewResults.Select(u => new InterviewResult
                {
                    Score = u.Score
                }).ToList()
            });

            return await query.ToListAsync();
        }
    }
}
