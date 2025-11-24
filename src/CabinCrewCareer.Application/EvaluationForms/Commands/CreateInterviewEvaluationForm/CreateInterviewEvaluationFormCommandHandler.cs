using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

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

            if (request.Type == (int)CampaignType.Recruitment)
            {
                if (finalScore > 120)
                {
                    form.IsPassed = true;
                }

                await _unitOfWork.EvaluationForms.AddAsync(form);

                await _unitOfWork.CompleteAsync();

                return new Response<string>(
                    code: (int)ResponseCode.CREATED_SUCCESS,
                    data: ResponseCode.CREATED_SUCCESS.ToString(),
                    message: ResponseCode.CREATED_SUCCESS.GetDescription());
            }

            if (finalScore > 70) form.IsPassed = true;

            await _unitOfWork.EvaluationForms.AddAsync(form);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(
                code: (int)ResponseCode.CREATED_SUCCESS,
                data: ResponseCode.CREATED_SUCCESS.ToString(),
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
