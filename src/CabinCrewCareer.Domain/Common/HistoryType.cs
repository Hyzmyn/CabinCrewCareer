using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum HistoryType
    {
        [Description("Ongoing")]
        Ongoing = 1,
        [Description("Completed")]
        Completed,
    }
}
