using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetTestSessionById
{
    public class GetTestSessionByIdQuery : IRequest<Response<GetTestSessionResponse>>
    {
        public int TestSessionId { get; set; }
    }
}
