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
        Canceled = 4,
        [Description("Published")]
        Published = 5,
        [Description("Ongoing")]
        Ongoing = 6,
        [Description("Upcoming")]
        Upcoming = 7,
        [Description("Ended")]
        Ended = 8
    }
}
