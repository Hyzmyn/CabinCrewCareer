namespace CabinCrewCareer.Application.TestSessions.Common
{
    public class SubmitTestSessionResponse
    {
        public int TestSessionId { get; set; }
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalScore { get; set; }
        public int MaxScore { get; set; }
        public bool Status { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public List<SubmittedAnswerDto> SubmittedAnswers { get; set; } = [];
        public string Message { get; set; } = string.Empty;
    }

    public class SubmittedAnswerDto
    {
        public int AnswerId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionContent { get; set; } = string.Empty;
        public int QuestionScore { get; set; }
        public int? SelectedOptionId { get; set; }
        public string? SelectedOptionContent { get; set; }
        public bool IsCorrect { get; set; }
        public int Score { get; set; }
        public string? EssayAnswer { get; set; }
        public string? AnswerAudioFileURL { get; set; }
    }
}
