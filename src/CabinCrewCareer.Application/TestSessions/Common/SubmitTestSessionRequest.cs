namespace CabinCrewCareer.Application.TestSessions.Common
{
    public class SubmitTestSessionRequest
    {
        public int TestId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<TestAnswerDto> Answers { get; set; } = [];
    }

    public class TestAnswerDto
    {
        // For input (when submitting)
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; } // For Listening/Practical - which option user selected
        public string? EssayAnswer { get; set; } // For Speaking - text answer if any
        public string? AnswerAudioFileURL { get; set; } // For Speaking - audio file URL

        // For output (when retrieving)
        public int AnswerId { get; set; }
        public string SelectedOptionContent { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int Score { get; set; }
        public string QuestionContent { get; set; } = string.Empty;
    }
}
