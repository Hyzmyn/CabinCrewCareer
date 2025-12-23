namespace CabinCrewCareer.Application.AirlinePartners.Common
{
    public class GetAirlinePartnerResponse
    {
        public int PartnerId { get; set; }

        public string PartnerName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
    }
}
