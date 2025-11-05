using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum AvailableRole
    {
        [Description("Admin")]
        Admin = 1,
        [Description("Director")]
        Director,

        [Description("Senior Recruiter")]
        SeniorRecruiter,

        [Description("Recruiter")]
        Recruiter,

        [Description("Examiner")]
        Examiner,

        [Description("Cabin Crew")]
        CabinCrew,

        [Description("Candidate")]
        Candidate,

        [Description("Airline Partner")]
        AirlinePartner,
    }
}
