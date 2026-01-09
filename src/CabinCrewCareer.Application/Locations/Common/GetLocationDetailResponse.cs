namespace CabinCrewCareer.Application.Locations.Common
{
    public class GetLocationDetailResponse
    {
        public int LocationId { get; set; }

        public string Address { get; set; } = string.Empty;

        public int WardId { get; set; }

        public int CityId { get; set; }
    }
}
