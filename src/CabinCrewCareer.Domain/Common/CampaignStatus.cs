using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum CampaignStatus
    {
        [Description("Draft")]
        Draft = 0,
        [Description("Pending")]
        Pending = 1,
        [Description("Approved")]
        Approved = 2,
        [Description("Rejected")]
        Rejected = 3,
        [Description("Canceled")]
        Canceled = 4
    }
}
