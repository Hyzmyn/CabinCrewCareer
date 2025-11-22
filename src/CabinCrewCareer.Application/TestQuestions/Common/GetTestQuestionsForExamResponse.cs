namespace CabinCrewCareer.Application.TestQuestions.Common
{
    public class GetTestQuestionsForExamResponse
    {
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public int DurationInMinutes { get; set; }
        public string? AudioFileURL { get; set; }
        public int TotalQuestions { get; set; }
        public List<ExamQuestionDto> Questions { get; set; } = [];
    }

    public class ExamQuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionContent { get; set; } = string.Empty;
        public int Score { get; set; }
        public int OrderNumber { get; set; }
        public List<ExamOptionDto>? Options { get; set; } = [];
    }

    public class ExamOptionDto
    {
        public int OptionId { get; set; }
        public string OptionContent { get; set; } = string.Empty;
        // Note: IsCorrect is intentionally NOT included for security
        // Students should not see which option is correct during the exam
    }
}
