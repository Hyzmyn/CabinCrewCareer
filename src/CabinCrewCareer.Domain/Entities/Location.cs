using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class Location
    {
        public int LocationId { get; set; }

        public CampaignType CampaignType { get; set; }

        public string Address { get; set; } = string.Empty;

        public int? WardId { get; set; }

        public virtual Ward Ward { get; set; } = null!;
    }
}
