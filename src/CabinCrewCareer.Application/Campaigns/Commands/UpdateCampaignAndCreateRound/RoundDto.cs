namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignAndCreateRound
{
    public class RoundDto
    {
        public string RoundName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TargetQuantity { get; set; }
        public DateTime? RoundStartDate { get; set; }
        public DateTime? RoundEndDate { get; set; }
    }
}
