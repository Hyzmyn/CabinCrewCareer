namespace CabinCrewCareer.Domain.Entities
{
    public class RoundActivity
    {
        public int ActivityId { get; set; }

        // Foreign Keys
        public int? RoundId { get; set; }
        public virtual Round Round { get; set; } = null!;

        public int? UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
