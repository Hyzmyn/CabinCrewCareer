namespace CabinCrewCareer.Application.Configurations.Common
{
    public class GetConfigurationResponse
    {
        public int RoundConfigurationId { get; set; }

        public string RoundType { get; set; } = string.Empty;

        public string CampaignType { get; set; } = string.Empty;

        public int Benchmark { get; set; }

        public string EffectiveDate { get; set; } = string.Empty;
    }
}
