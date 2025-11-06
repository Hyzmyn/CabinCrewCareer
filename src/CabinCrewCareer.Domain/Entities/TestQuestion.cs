namespace CabinCrewCareer.Domain.Entities
{
    public class TestQuestion
    {
        public int QuestionId { get; set; }

        public string QuestionContent { get; set; } = string.Empty;

        public int Score { get; set; }

        public int OrderNumber { get; set; }

        // Foreign Keys
        public int? TestId { get; set; }
        public virtual Test Test { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<TestOption> TestOptions { get; set; } = [];
    }
}
