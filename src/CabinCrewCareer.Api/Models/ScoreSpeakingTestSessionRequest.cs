namespace CabinCrewCareer.Api.Models
{
    public class ScoreSpeakingTestSessionRequest
    {
        /// <summary>
        /// Dictionary of scoring criteria and their scores
        /// Key: Criteria name (e.g., "Pronunciation", "Fluency", "Grammar")
        /// Value: Score for that criteria (non-negative integer)
        /// </summary>
        public Dictionary<string, int> CriteriaScores { get; set; } = new();
    }
}
