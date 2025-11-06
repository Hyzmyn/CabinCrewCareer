namespace CabinCrewCareer.Domain.Entities
{
    public class TestAnswer
    {
        public int AnswerId { get; set; }

        public int Score { get; set; }

        public bool IsCorrect { get; set; }

        public string EssayAnswer { get; set; } = string.Empty;

        // Foreign Keys
        public int? TestSessionId { get; set; }
        public virtual TestSession TestSession { get; set; } = null!;

        public int? SelectedOptionId { get; set; }
        public virtual TestOption SelectedOption { get; set; } = null!;

        //public int? QuestionId { get; set; }
        //public virtual TestQuestion TestQuestion { get; set; } = null!;

        // Navigation Properties
    }
}
