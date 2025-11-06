namespace CabinCrewCareer.Domain.Entities
{
    public class TestOption
    {
        public int OptionId { get; set; }

        public string OptionContent { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        // Foreign Keys
        public int? QuestionId { get; set; }
        public virtual TestQuestion TestQuestion { get; set; } = null!;

        public virtual TestAnswer? TestAnswer { get; set; } = null;

        // Navigation Properties
    }
}
