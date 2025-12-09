using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class Requirement
    {
        public int RequirementId { get; set; }

        public CampaignType CampaignType { get; set; }

        public virtual ICollection<CampaignRequest> CampaignRequests { get; set; } = [];

        public virtual ICollection<RequirementItem> RequirementItems { get; set; } = [];
    }
}
