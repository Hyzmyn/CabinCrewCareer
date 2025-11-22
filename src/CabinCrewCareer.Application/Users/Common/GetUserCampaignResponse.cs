namespace CabinCrewCareer.Application.Users.Common
{
    public class GetUserCampaignResponse
    {
        public int CampaignRoundId { get; set; }
        public string RoundName { get; set; } = string.Empty;
        public string CampaignName { get; set; } = string.Empty;
        public string RoundStatus { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AirlinePartner { get; set; } = string.Empty;
        public List<GetRoundActivityResponse> Rounds { get; set; } = [];
    }

    public class GetRoundActivityResponse
    {
        public int RoundId { get; set; }
        public string RoundName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
