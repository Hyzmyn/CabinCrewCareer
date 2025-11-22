using CabinCrewCareer.Application.UserApplications.Commands.Requests;
using MediatR;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    using CabinCrewCareer.Application.UserApplications.Common;
    public class SubmitUserApplicationCommand : IRequest<Response<UserApplicationResponse>>
    {
        public CreateUserApplicationRequest Request { get; set; }
        public SubmitUserApplicationCommand(CreateUserApplicationRequest request)
        {
            Request = request;
        }
    }
}
