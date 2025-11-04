namespace CabinCrewCareer.Domain.Entities
{
    public class City
    {
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;

        // Navigation Properties
        public virtual ICollection<Ward> Wards { get; set; } = [];
    }
}
