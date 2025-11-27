using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetTestAnswersBySessionId
{
    public class GetTestAnswersBySessionIdQuery : IRequest<Response<List<TestAnswerDto>>>
    {
        public int TestSessionId { get; set; }
    }
}
