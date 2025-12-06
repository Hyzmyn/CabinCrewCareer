using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using FluentValidation;
using MediatR;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class UpdateFlightExperienceCommandHandler(
        IUnitOfWork unitOfWork,
        IValidator<UpdateFlightExperienceCommand> validator) : IRequestHandler<UpdateFlightExperienceCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IValidator<UpdateFlightExperienceCommand> _validator = validator;

        public async Task<Response<string>> Handle(UpdateFlightExperienceCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new CabinCrewCareer.Application.Common.Exceptions.ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Get UserApplicationSnapshot by ID
            var applicationSnapshot = await _unitOfWork.UserApplicationSnapshots.GetUserApplicationSnapshotByIdAsync(request.ApplicationSnapshotId);

            if (applicationSnapshot == null)
            {
                errorMessages.Add("Application snapshot not found.");
                throw new ApiException((int)ResponseCode.GET_UNSUCC, "Application snapshot not found.", string.Join(", ", errorMessages));
            }

            // Update TotalFlightHours and Experience
            applicationSnapshot.TotalFightHours = request.TotalFlightHours;
            applicationSnapshot.Experience = request.Experience;

            // Save changes
            await _unitOfWork.UserApplicationSnapshots.UpdateAsync(applicationSnapshot);
            await _unitOfWork.CompleteAsync();

            return new Response<string>(
                code: (int)ResponseCode.UPDATED_SUCCESS,
                message: "Flight experience updated successfully.",
                data: $"Updated Application Snapshot ID: {request.ApplicationSnapshotId}"
            );
        }
    }
}
