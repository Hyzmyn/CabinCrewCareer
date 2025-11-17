using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum ActivityStatus
    {
        [Description("Ongoing")]
        Ongoing = 1,
        [Description("Passed")]
        Passed = 2,
        [Description("Failed")]
        Failed = 3,
    }
}
