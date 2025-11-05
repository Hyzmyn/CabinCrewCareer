using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum AvailableAirlinePartner
    {
        [Description("VietJetAir")]
        VietJetAir = 1,
        [Description("Vietnam Airlines")]
        VietnameAirlines = 3,

        [Description("Bamboo Airways")]
        BambooAirways = 4,

        [Description("Sun PhuQuoc Airways")]
        SunPhuQuocAirways = 5,
    }
}
