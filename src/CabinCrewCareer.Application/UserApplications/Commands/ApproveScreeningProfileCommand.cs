using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public record ApproveScreeningProfileCommand(
        int ActivityId,
        int Status
    ) : IRequest<Response<int?>>
    {
        public class ApproveScreeningProfileDto
        {
            public int Status { get; set; }
        }
    }
}
