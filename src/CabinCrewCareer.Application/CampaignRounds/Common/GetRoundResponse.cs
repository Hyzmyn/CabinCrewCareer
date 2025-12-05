namespace CabinCrewCareer.Application.CampaignRounds.Common
{
    public class GetRoundResponse
    {
        public int RoundId { get; set; }

        public string RoundName { get; set; } = string.Empty;

        public int TestId { get; set; }
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
    }
}
