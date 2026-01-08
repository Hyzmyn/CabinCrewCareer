namespace CabinCrewCareer.Domain.Entities
{
    public class Ward
    {
        public int WardId { get; set; }
        public string WardName { get; set; } = string.Empty;

        // Foreign Key
        public int? CityId { get; set; }
        public virtual City City { get; set; } = null!;

        // Navigation
        public virtual ICollection<User> Users { get; set; } = [];
        public virtual ICollection<AirlinePartner> AirlinePartners { get; set; } = [];
        public virtual ICollection<Location> Locations { get; set; } = [];
    }
}
