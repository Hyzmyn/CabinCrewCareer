namespace CabinCrewCareer.Api.Models
{
    public class ScoreTestAnswerRequest
    {
        /// <summary>
        /// Dictionary of answer IDs and their scores
        /// Key: Answer ID
        /// Value: Score criteria and correctness information
        /// </summary>
        public Dictionary<int, AnswerScoreInfo> AnswerScores { get; set; } = new();
    }

    public class AnswerScoreInfo
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
