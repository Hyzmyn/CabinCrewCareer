using Microsoft.AspNetCore.Mvc;

namespace CabinCrewCareer.Api.Models
{
    /// <summary>
    /// Request model for submitting speaking test
    /// </summary>
    public class SubmitSpeakingTestRequest
    {
        /// <summary>
        /// Test ID
        /// </summary>
        [FromForm(Name = "testId")]
        public int TestId { get; set; }

        /// <summary>
        /// Test start time (ISO 8601: 2025-11-22T10:00:00Z)
        /// </summary>
        [FromForm(Name = "startTime")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Test end time (ISO 8601: 2025-11-22T10:30:00Z)
        /// </summary>
        [FromForm(Name = "endTime")]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Comma-separated list of question IDs (e.g., "137,138,139")
        /// </summary>
        [FromForm(Name = "questionIds")]
        public string QuestionIds { get; set; } = string.Empty;

        /// <summary>
        /// List of audio files (.mp3) - must match number of questions
        /// </summary>
        [FromForm(Name = "audioFiles")]
        public List<IFormFile> AudioFiles { get; set; } = new();
    }
}
