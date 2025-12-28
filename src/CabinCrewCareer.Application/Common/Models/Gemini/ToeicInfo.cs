using System;

namespace CabinCrewCareer.Application.Common.Models
{
    public class ToeicInfo
    {
        public string CertificateType { get; set; } = string.Empty;
        public string CitizenId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string DateOfBirth { get; set; }
        public string TestDate { get; set; }
        public string ValidUntil { get; set; }
        public int ListeningScore { get; set; }
        public int ReadingScore { get; set; }
        public int TotalScore { get; set; }
    }
}
