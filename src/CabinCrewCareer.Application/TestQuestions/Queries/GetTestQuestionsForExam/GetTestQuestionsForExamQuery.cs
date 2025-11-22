using CabinCrewCareer.Application.TestQuestions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestQuestions.Queries.GetTestQuestionsForExam
{
    public record GetTestQuestionsForExamQuery(int TestId, string JoinCode) : IRequest<Response<GetTestQuestionsForExamResponse>>;
}
