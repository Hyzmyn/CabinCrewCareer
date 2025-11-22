using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum RoundType
    {
        [Description("Screening")]
        Screening = 1,
        [Description("Appearance")]
        Appearance = 2,
        [Description("Flight Hours Confirmation")]
        FlightHoursConfirmation = 3,
        [Description("English Listening Test")]
        EnglishListeningTest = 4,
        [Description("English Speaking Test")]
        EnglishSpeakingTest = 5,
        [Description("Practical Test")]
        PracticalTest = 6,
        [Description("Interview")]
        Interview = 7,
        [Description("Final")]
        Final = 8
    }
}
