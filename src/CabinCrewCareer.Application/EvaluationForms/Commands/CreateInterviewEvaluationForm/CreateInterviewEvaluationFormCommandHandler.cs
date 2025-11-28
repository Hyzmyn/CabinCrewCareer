using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.EvaluationForms.Commands.CreateInterviewEvaluationForm
{
    public class CreateInterviewEvaluationFormCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateInterviewEvaluationFormCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        public async Task<Response<string>> Handle(CreateInterviewEvaluationFormCommand request, CancellationToken cancellationToken)
        {
            var finalScore = 0;

            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            EvaluationForm form = new()
            {
                ActivityId = request.ActivityId,
                Comment = request.Comment,
                ExaminerId = Int32.Parse(currentUserId),
                Type = EvaluationType.Interview,
                EvaluatedDate = _dateTimeProvider.UtcNow
            };

            foreach (var item in request.Choices)
            {
                form.InterviewResults.Add(new InterviewResult
                {
                    Score = item.Score,
                    InterviewCriteriaItemId = item.InterviewCriteriaItemId,
                    Comment = item.Comment
                });

                finalScore += item.Score;
            }

            var roundActivityQuery = await _unitOfWork.RoundActivites.GetAsync(
                expression: x => x.ActivityId == request.ActivityId,
                includeFunc: x => x.Include(x => x.Round));

            var roundActivity = await roundActivityQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            var evaluationForms = await _unitOfWork.EvaluationForms.GetInterviewFormsAsync(request.ActivityId);

            if (evaluationForms.Count == 3) throw new ApiException(ResponseCode.EXCEED_FORM_LIMIT);

            if (request.Type == (int)CampaignType.Recruitment)
            {
                if (finalScore > 120)
                {
                    form.IsPassed = true;
                }

                if (roundActivity is not null)
                {
                    if (form.IsPassed)
                    {
                        if (evaluationForms.Count(x => x.IsPassed) == 2)
                        {
                            roundActivity.Status = ActivityStatus.Passed;
                        }

                        var finalRoundQuery = await _unitOfWork.Rounds.GetAsync(
                                expression: x => x.CampaignRoundId == roundActivity.Round.CampaignRoundId && x.RoundType == RoundType.Final);

                        var finalRound = await finalRoundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                        if (finalRound is not null) await _unitOfWork.RoundActivites.AddAsync(new RoundActivity { RoundId = finalRound.RoundId, UserId = roundActivity.UserId, Status = ActivityStatus.Ongoing });
                    }
                    else
                    {
                        if (evaluationForms.Count == 2) roundActivity.Status = ActivityStatus.Failed;
                    }

                    await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);
                }

                await _unitOfWork.EvaluationForms.AddAsync(form);

                await _unitOfWork.CompleteAsync();

                return new Response<string>(
                    code: (int)ResponseCode.CREATED_SUCCESS,
                    data: ResponseCode.CREATED_SUCCESS.ToString(),
                    message: ResponseCode.CREATED_SUCCESS.GetDescription());
            }

            if (finalScore > 70) form.IsPassed = true;

            if (roundActivity is not null)
            {
                if (form.IsPassed)
                {
                    if (evaluationForms.Count(x => x.IsPassed) == 2)
                    {
                        roundActivity.Status = ActivityStatus.Passed;
                    }

                    var finalRoundQuery = await _unitOfWork.Rounds.GetAsync(
                            expression: x => x.CampaignRoundId == roundActivity.Round.CampaignRoundId && x.RoundType == RoundType.Final);

                    var finalRound = await finalRoundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (finalRound is not null) await _unitOfWork.RoundActivites.AddAsync(new RoundActivity { RoundId = finalRound.RoundId, UserId = roundActivity.UserId, Status = ActivityStatus.Ongoing });
                }
                else
                {
                    if (evaluationForms.Count == 2) roundActivity.Status = ActivityStatus.Failed;
                }

                await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);
            }

            await _unitOfWork.EvaluationForms.AddAsync(form);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(
                code: (int)ResponseCode.CREATED_SUCCESS,
                data: ResponseCode.CREATED_SUCCESS.ToString(),
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
