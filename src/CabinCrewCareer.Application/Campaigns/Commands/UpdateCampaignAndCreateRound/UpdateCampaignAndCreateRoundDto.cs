namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignAndCreateRound
{
    public class UpdateCampaignAndCreateRoundDto
    {
        // Campaign fields
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        // Thêm các trường campaign khác nếu cần

        // List of rounds to create
        public List<RoundDto>? Rounds { get; set; }
        // List of round dates for each round (optional, for advanced use)
        public List<RoundDateDto>? RoundDates { get; set; }
    }
}
