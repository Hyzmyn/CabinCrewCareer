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

            var evaluationQuery = await _unitOfWork.EvaluationForms.GetAsync(x => x.ActivityId == request.ActivityId);

            var evaluation = evaluationQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (evaluation is not null) throw new ApiException(ResponseCode.CANDIDATE_HAS_BEEN_EVALUATED);

            EvaluationForm form = new()
            {
                ActivityId = request.ActivityId,
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

            await _unitOfWork.EvaluationForms.AddAsync(form);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(
                code: (int)ResponseCode.CREATED_SUCCESS,
                data: ResponseCode.CREATED_SUCCESS.ToString(),
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
