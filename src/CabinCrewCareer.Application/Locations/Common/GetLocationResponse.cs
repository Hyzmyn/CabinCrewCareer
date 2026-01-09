namespace CabinCrewCareer.Application.Locations.Common
{
    public class GetLocationResponse
    {
        public int LocationId { get; set; }

        public string CampaignType { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int WardId { get; set; }

        public int CityId { get; set; }
    }
}
