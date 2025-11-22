using CabinCrewCareer.Application.UserApplications.Commands.Requests;
using MediatR;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    using CabinCrewCareer.Application.UserApplications.Common;
    public class UpdateUserApplicationCommand : IRequest<Response<UserApplicationResponse>>
    {
        public int ApplicationId { get; set; }
        public UpdateUserApplicationRequest Request { get; set; }
        public UpdateUserApplicationCommand(int applicationId, UpdateUserApplicationRequest request)
        {
            ApplicationId = applicationId;
            Request = request;
        }
    }
}
