namespace CabinCrewCareer.Application.TestSessions.Common
{
    // For Listening and Practical tests - Multiple choice
    public class SubmitMultipleChoiceTestRequest
    {
        public int TestId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<MultipleChoiceAnswerDto> Answers { get; set; } = [];
    }

    public class MultipleChoiceAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; } // Required for multiple choice
    }
}
