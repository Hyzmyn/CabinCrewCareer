using System.Linq;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.UserApplications.Common;
using CabinCrewCareer.Domain.Common;
using MediatR;
using CabinCrewCareer.Application.Common.Enums;
using System.Collections.Generic;

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
            var application = await _unitOfWork.UserApplications.GetUserApplicationByUserIdAsync(request.UserId);
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
                ApplicationId = application.ApplicationId,
                UserId = application.UserId,
                Status = application.ApplicationStatus.ToString(),
                SubmissionDate = application.SubmissionDate.HasValue ? application.SubmissionDate.Value.GetFormatDateTime() : null,
                Experience = application.Experience,
                Height = application.Height,
                Weight = application.Weight,
                EnglishDegreeNumber = application.EnglishDegreeNumber,
                EndDate = application.EndDate.HasValue ? application.EndDate.Value.ToString("dd/MM/yyyy") : null,
                CitizenId = application.CitizenId,
                EnglishTestDate = application.EnglishTestDate.HasValue ?  application.EnglishTestDate.Value.ToString("dd/MM/yyyy") : null,
                ListeningScore = application.ListeningScore,
                ReadingScore = application.ReadingScore,
                TotalScore = application.TotalScore,    
                Documents = application.Documents?.Select(d => new DocumentResponse
                {
                    DocumentId = d.DocumentId,
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
