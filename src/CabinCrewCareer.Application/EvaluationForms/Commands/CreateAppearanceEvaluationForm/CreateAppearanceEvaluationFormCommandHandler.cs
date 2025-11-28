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

namespace CabinCrewCareer.Application.EvaluationForms.Commands.CreateAppearanceEvaluationForm
{
    public class CreateAppearanceEvaluationFormCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateAppearanceEvaluationFormCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        public async Task<Response<string>> Handle(CreateAppearanceEvaluationFormCommand request, CancellationToken cancellationToken)
        {
            int count = 0;

            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            var evaluationQuery = await _unitOfWork.EvaluationForms.GetAsync(expression: x => x.ActivityId == request.ActivityId);

            var evaluation = await evaluationQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (evaluation is not null) throw new ApiException(ResponseCode.CANDIDATE_HAS_BEEN_EVALUATED);

            EvaluationForm form = new()
            {
                ActivityId = request.ActivityId,
                Comment = request.Comment,
                ExaminerId = Int32.Parse(currentUserId),
                Type = EvaluationType.Appearance,
                EvaluatedDate = _dateTimeProvider.UtcNow
            };

            foreach (var item in request.Choices)
            {
                form.AppearanceResults.Add(new AppearanceResult
                {
                    IsPassed = item.IsPassed,
                    ScoringCriteriaItemId = item.ScoringCriteriaItemId
                });

                if (item.IsPassed) count++;
            }

            if (count > 7) form.IsPassed = true;

            var roundActivityQuery = await _unitOfWork.RoundActivites.GetAsync(
                expression: x => x.ActivityId == request.ActivityId,
                includeFunc: x => x.Include(x => x.Round));

            var roundActivity = await roundActivityQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (roundActivity is not null)
            {
                if (form.IsPassed)
                {
                    roundActivity.Status = ActivityStatus.Passed;

                    var listeningRoundQuery = await _unitOfWork.Rounds.GetAsync(
                        expression: x => x.CampaignRoundId == roundActivity.Round.CampaignRoundId && x.RoundType == RoundType.EnglishListeningTest);

                    var listeningRound = await listeningRoundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (listeningRound is not null) await _unitOfWork.RoundActivites.AddAsync(new RoundActivity { RoundId = listeningRound.RoundId, UserId = roundActivity.UserId, Status = ActivityStatus.Ongoing });

                    var speakingRoundQuery = await _unitOfWork.Rounds.GetAsync(expression: x => x.CampaignRoundId == roundActivity.Round.CampaignRoundId && x.RoundType == RoundType.EnglishSpeakingTest);

                    var speakingRound = await speakingRoundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (speakingRound is not null) await _unitOfWork.RoundActivites.AddAsync(new RoundActivity { RoundId = speakingRound.RoundId, UserId = roundActivity.UserId, Status = ActivityStatus.Ongoing });
                }
                else
                {
                    roundActivity.Status = ActivityStatus.Failed;
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
