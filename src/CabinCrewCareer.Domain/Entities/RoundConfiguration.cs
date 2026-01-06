using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class RoundConfiguration
    {
        public int RoundConfigurationId { get; set; }

        public RoundConfigurationType ConfigurationType { get; set; }

        public CampaignType CampaignType { get; set; }

        public int Benchmark { get; set; }

        public DateOnly? EffectiveDate { get; set; }
    }
}
