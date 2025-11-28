namespace CabinCrewCareer.Api.Models
{
    public class UpdateEnquiryRequestScoreRequest
    {
        /// <summary>
        /// Dictionary of answer IDs and their scores after re-evaluation
        /// Key: Answer ID
        /// Value: Score criteria and correctness information
        /// </summary>
        public Dictionary<int, AnswerScoreInfoRequest> AnswerScores { get; set; } = new();

        /// <summary>
        /// New reason/comment from examiner after reviewing the enquiry request
        /// </summary>
        public string? NewReason { get; set; }
    }

    public class AnswerScoreInfoRequest
    {
        /// <summary>
        /// Dictionary of scoring criteria and their scores
        /// Key: Criteria name (e.g., "Pronunciation", "Fluency", "Grammar")
        /// Value: Score for that criteria (non-negative integer)
        /// </summary>
        public Dictionary<string, int> CriteriaScores { get; set; } = new();

        /// <summary>
        /// Whether the answer is correct or not
        /// </summary>
        public bool IsCorrect { get; set; }
    }
}
