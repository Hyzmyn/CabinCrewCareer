using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum TestType
    {
        [Description("English Listening")]
        EnglishListening = 1,
        [Description("English Speaking")]
        EnglishSpeaking = 2,
        [Description("Practical")]
        Practical = 3,
    }
}
