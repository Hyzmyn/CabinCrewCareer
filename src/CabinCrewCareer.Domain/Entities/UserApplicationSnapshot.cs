using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class UserApplicationSnapshot
    {
        public int ApplicationSnapshotId { get; set; }

        public ApplicationStatus ApplicationStatus { get; set; }

        public DateTime? SubmissionDate { get; set; }

        public string Experience { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public DateOnly? DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        public int Height { get; set; } = 0;

        public int Weight { get; set; } = 0;

        public string EnglishDegreeNumber { get; set; } = string.Empty;

        public DateOnly? EndDate { get; set; }

        // Thông tin bổ sung từ chứng chỉ tiếng Anh
        public string CitizenId { get; set; } = string.Empty; // Số Căn Cước công dân

        public DateOnly? EnglishTestDate { get; set; } // Ngày thi lấy bằng tiếng Anh

        public int ListeningScore { get; set; } = 0; // Điểm nghe

        public int ReadingScore { get; set; } = 0; // Điểm đọc

        public int TotalScore { get; set; } = 0; // Tổng điểm

        // Navigation Properties
        public virtual ICollection<DocumentSnapshot> DocumentSnapshots { get; set; } = [];
        public virtual ICollection<RoundActivity> RoundActivities { get; set; } = [];
    }
}
