using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.EnquiryRequests.Commands.UpdateEnquiryRequestScore
{
    public class UpdateEnquiryRequestScoreCommand : IRequest<Response<string>>
    {
        public int TestSessionId { get; set; }

        public Dictionary<int, AnswerScoreInfo> AnswerScores { get; set; } = new();

        public string? NewReason { get; set; }
    }

    public class AnswerScoreInfo
    {
        public Dictionary<string, int> CriteriaScores { get; set; } = new();
        public bool IsCorrect { get; set; }
    }
}
