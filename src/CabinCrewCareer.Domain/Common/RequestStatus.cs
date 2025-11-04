using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum RequestStatus
    {
        [Description("Pending")]
        Pending = 1,
        [Description("Approved")]
        Approved,
        [Description("Rejected")]
        Rejected
    }
}
