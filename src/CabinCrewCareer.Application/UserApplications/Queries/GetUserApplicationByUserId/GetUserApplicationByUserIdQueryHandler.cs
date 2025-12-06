using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.UserApplications.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.UserApplications.Queries.GetUserApplicationByUserId
{
    public class GetUserApplicationByUserIdQueryHandler : IRequestHandler<GetUserApplicationByUserIdQuery, Response<UserApplicationDetailResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserApplicationByUserIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<UserApplicationDetailResponse>> Handle(GetUserApplicationByUserIdQuery request, CancellationToken cancellationToken)
        {
            var applicationQuery = await _unitOfWork.RoundActivites.GetAsync(
                expression: x => x.ActivityId == request.UserId,
                includeFunc: x => x.Include(x => x.UserApplicationSnapshot).ThenInclude(x => x.DocumentSnapshots));

            var application = await applicationQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (application == null)
            {
                return new Response<UserApplicationDetailResponse>
                {
                    Code = 404,
                    Message = "Application not found.",
                    Data = null,
                    Errors = "Not found"
                };
            }

            // Map to response
            var response = new UserApplicationDetailResponse
            {
                ApplicationId = application.UserApplicationSnapshot.ApplicationSnapshotId,
                Status = application.Status.ToString(),
                SubmissionDate = application.UserApplicationSnapshot.SubmissionDate.HasValue ? application.UserApplicationSnapshot.SubmissionDate.Value.GetFormatDateTime() : null,
                Experience = application.UserApplicationSnapshot.Experience,
                TotalFlightHours = application.UserApplicationSnapshot.TotalFightHours,
                Email = application.UserApplicationSnapshot.Email,
                FullName = application.UserApplicationSnapshot.FullName,
                PhoneNumber = application.UserApplicationSnapshot.PhoneNumber,
                DateOfBirth = application.UserApplicationSnapshot.DateOfBirth.HasValue ? application.UserApplicationSnapshot.DateOfBirth.Value.GetFormatDate() : null,
                Gender = application.UserApplicationSnapshot.Gender.GetDescription(),
                Height = application.UserApplicationSnapshot.Height,
                Weight = application.UserApplicationSnapshot.Weight,
                EnglishDegreeNumber = application.UserApplicationSnapshot.EnglishDegreeNumber,
                EndDate = application.UserApplicationSnapshot.EndDate.HasValue ? application.UserApplicationSnapshot.EndDate.Value.ToString("dd/MM/yyyy") : null,
                CitizenId = application.UserApplicationSnapshot.CitizenId,
                EnglishTestDate = application.UserApplicationSnapshot.EnglishTestDate.HasValue ? application.UserApplicationSnapshot.EnglishTestDate.Value.ToString("dd/MM/yyyy") : null,
                ListeningScore = application.UserApplicationSnapshot.ListeningScore,
                ReadingScore = application.UserApplicationSnapshot.ReadingScore,
                TotalScore = application.UserApplicationSnapshot.TotalScore,
                Documents = application.UserApplicationSnapshot.DocumentSnapshots?.Select(d => new DocumentResponse
                {
                    DocumentId = d.DocumentSnapshotId,
                    DocumentURL = d.DocumentURL,
                    Type = d.Type.ToString(),
                    UploadDate = d.UploadDate.HasValue ? d.UploadDate.Value.ToString("dd/MM/yyyy") : null
                }).ToList() ?? new List<DocumentResponse>()
            };

            return new Response<UserApplicationDetailResponse>
            {
                Code = (int)ResponseCode.SUCCESS,
                Message = ResponseCode.SUCCESS.ToString(),
                Data = response,
                Errors = null
            };
        }
    }
}
