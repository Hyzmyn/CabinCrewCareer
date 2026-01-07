using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum CampaignType
    {
        [Description("All")]
        All = 0,
        [Description("Recruitment")]
        Recruitment,
        [Description("Promotion")]
        Promotion,
    }
}
