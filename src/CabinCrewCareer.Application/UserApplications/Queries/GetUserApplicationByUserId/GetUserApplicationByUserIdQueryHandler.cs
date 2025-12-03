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
            var applicationQuery = await _unitOfWork.UserApplicationSnapshots.GetAsync(
                expression: x => x.ApplicationSnapshotId == request.UserId,
                includeFunc: x => x.Include(x => x.DocumentSnapshots));

            var application = await applicationQuery.FirstOrDefaultAsync();

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
                ApplicationId = application.ApplicationSnapshotId,
                Status = application.ApplicationStatus.ToString(),
                SubmissionDate = application.SubmissionDate.HasValue ? application.SubmissionDate.Value.GetFormatDateTime() : null,
                Experience = application.Experience,
                Email = application.Email,
                FullName = application.FullName,
                PhoneNumber = application.PhoneNumber,
                DateOfBirth = application.DateOfBirth.HasValue ? application.DateOfBirth.Value.GetFormatDate() : null,
                Gender = application.Gender.GetDescription(),
                Height = application.Height,
                Weight = application.Weight,
                EnglishDegreeNumber = application.EnglishDegreeNumber,
                EndDate = application.EndDate.HasValue ? application.EndDate.Value.ToString("dd/MM/yyyy") : null,
                CitizenId = application.CitizenId,
                EnglishTestDate = application.EnglishTestDate.HasValue ? application.EnglishTestDate.Value.ToString("dd/MM/yyyy") : null,
                ListeningScore = application.ListeningScore,
                ReadingScore = application.ReadingScore,
                TotalScore = application.TotalScore,
                Documents = application.DocumentSnapshots?.Select(d => new DocumentResponse
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
