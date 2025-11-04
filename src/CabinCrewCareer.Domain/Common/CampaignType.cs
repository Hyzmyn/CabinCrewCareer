using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum CampaignType
    {
        [Description("Recruitment")]
        Recruitment = 1,
        [Description("Promotion")]
        Promotion,
    }
}
