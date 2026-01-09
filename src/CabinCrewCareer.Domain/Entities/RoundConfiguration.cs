namespace CabinCrewCareer.Domain.Entities
{
    public class RoundConfiguration
    {
        public int RoundConfigurationId { get; set; }

        //public RoundConfigurationType ConfigurationType { get; set; }

        //public CampaignType CampaignType { get; set; }

        public int Benchmark { get; set; }

        public bool Status { get; set; } = true;

        public DateOnly? EffectiveDate { get; set; }

        public DateOnly? ExpiredDate { get; set; }
        // Foreign Keys
        public int? RoundTypeId { get; set; }
        public virtual TypeRound RoundType { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<EvaluationForm> EvaluationForms { get; set; } = [];
    }
}
