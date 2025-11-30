using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetTestAnswersWithCriteria
{
    public class GetTestAnswersWithCriteriaQuery : IRequest<Response<List<TestAnswerWithCriteriaResponse>>>
    {
        public int TestSessionId { get; set; }
    }
}
