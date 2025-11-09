namespace CabinCrewCareer.Application.Campaigns.Commands.CreateCampaignRound
{
    public class CreateCampaignRoundDto
    {
        public string RoundName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TargetQuantity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        // Thêm các trường khác nếu cần
    }
}
