using CabinCrewCareer.Application.TestQuestions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestQuestions.Queries.GetTestQuestions
{
    public record GetTestQuestionsQuery(int TestId) : IRequest<Response<GetTestQuestionsResponse>>;
}
