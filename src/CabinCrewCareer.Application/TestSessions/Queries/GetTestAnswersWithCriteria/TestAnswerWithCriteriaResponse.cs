using System.Text.Json.Serialization;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetTestAnswersWithCriteria
{
    public class TestAnswerWithCriteriaResponse
    {
        public int AnswerId { get; set; }
        public int Score { get; set; }
        public bool IsCorrect { get; set; }
        public string? EssayAnswer { get; set; }
        public string? AnswerAudioFileURL { get; set; }
        public int? SelectedOptionId { get; set; }
        
        /// <summary>
        /// Parsed criteria scores from EssayAnswer JSON
        /// </summary>
        public Dictionary<string, int>? CriteriaScores { get; set; }
        
        /// <summary>
        /// Student's original essay content (if exists in EssayAnswer JSON)
        /// </summary>
        public string? StudentAnswer { get; set; }
        
        /// <summary>
        /// Question information
        /// </summary>
        public QuestionInfo? Question { get; set; }
    }

    public class QuestionInfo
    {
        public int QuestionId { get; set; }
        public string QuestionContent { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
        public int MaxScore { get; set; }
    }
}
