using CabinCrewCareer.Application.UserApplications.Commands.Requests;
using MediatR;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    using CabinCrewCareer.Application.UserApplications.Common;
    public class CreateUserApplicationDraftCommand : IRequest<Response<UserApplicationResponse>>
    {
        public CreateUserApplicationRequest Request { get; set; }
        public CreateUserApplicationDraftCommand(CreateUserApplicationRequest request)
        {
            Request = request;
        }
    }
}
