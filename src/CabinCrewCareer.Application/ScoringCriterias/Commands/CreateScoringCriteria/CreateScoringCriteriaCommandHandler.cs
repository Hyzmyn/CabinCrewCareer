using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.ScoringCriterias.Commands.CreateScoringCriteria
{
    public class CreateScoringCriteriaCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateScoringCriteriaCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(CreateScoringCriteriaCommand request, CancellationToken cancellationToken)
        {
            ScoringCriteriaItem scoringItem = new()
            {
                ItemType = request.CreateScoringCriteriaRequest.ItemType,
                Text = request.CreateScoringCriteriaRequest.Text,
                EnglishText = request.CreateScoringCriteriaRequest.EnglishText,
                ScoringCriteriaId = request.ScoringCriteriaId,
                ScoringCriteriaDetails = [.. request.CreateScoringCriteriaRequest.Details.Select(x => new ScoringCriteriaDetail
                {
                    DetailText = x.DetailText
                })]
            };

            await _unitOfWork.ScoringCriteriaItems.AddAsync(scoringItem);

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.CREATED_UNSUCC,
                data: ResponseCode.CREATED_SUCCESS.ToString(),
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
