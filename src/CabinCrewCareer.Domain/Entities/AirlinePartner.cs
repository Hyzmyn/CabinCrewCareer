namespace CabinCrewCareer.Domain.Entities
{
    public class AirlinePartner
    {
        public int PartnerId { get; set; }

        public string PartnerName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        // Foreign Keys
        public int? WardId { get; set; }
        public virtual Ward Ward { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<User> Users { get; set; } = [];
    }
}
