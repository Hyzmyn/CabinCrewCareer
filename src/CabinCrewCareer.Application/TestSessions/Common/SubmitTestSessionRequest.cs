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
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; } // For Listening/Practical - which option user selected
        public string? EssayAnswer { get; set; } // For Speaking - text answer if any
        public string? AnswerAudioFileURL { get; set; } // For Speaking - audio file URL
    }
}
