using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum RoundStatus
    {
        [Description("Ongoing")]
        Ongoing = 1,
        [Description("Upcoming")]
        Upcoming,
        [Description("Ended")]
        Ended,
        [Description("Failed")]
        Failed,
        [Description("Passed")]
        Passed
    }
}
