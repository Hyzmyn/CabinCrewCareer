namespace CabinCrewCareer.Application.CampaignRounds.Common
{
    public class GetCampaignRoundResponse
    {
        public int CampaignRoundId { get; set; }

        public string RoundName { get; set; } = string.Empty;

        public string StartDate { get; set; } = string.Empty;

        public string EndDate { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int TargetQuantity { get; set; }

        public int ActualQuantiy { get; set; }
    }
}
