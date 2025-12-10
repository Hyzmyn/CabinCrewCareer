using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Requirements.Commands.CreateRequirement
{
    public class CreateRequirementCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateRequirementCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(CreateRequirementCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.RequirementItems.AddAsync(new RequirementItem
            {
                Title = request.Title,
                Description = request.Description,
                RequirementId = request.RequirementId,
            });

            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.CREATED_UNSUCC,
                data: ResponseCode.CREATED_SUCCESS.ToString(),
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
