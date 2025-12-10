using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.InterviewCriterias.Commands.CreateInterviewCriteria
{
    public class CreateInterviewCriteriaCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateInterviewCriteriaCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(CreateInterviewCriteriaCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.InterviewCriteriaItems.AddAsync(new InterviewCriteriaItem
            {
                Criteria = request.Criteria,
                InterviewCriteriaId = request.InterviewCriteriaId
            });

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.CREATED_UNSUCC,
                data: ResponseCode.CREATED_SUCCESS.ToString(),
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
