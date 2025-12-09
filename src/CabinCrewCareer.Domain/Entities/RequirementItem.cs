namespace CabinCrewCareer.Domain.Entities
{
    public class RequirementItem
    {
        public int RequirementItemId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int? RequirementId { get; set; }
        public virtual Requirement Requirement { get; set; } = null!;
    }
}
