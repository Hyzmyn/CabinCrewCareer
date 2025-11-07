using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Application.Users.Common
{
    public class GetUserDetailResponse
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string DateOfBirth { get; set; } = string.Empty;

        public Gender Gender { get; set; }

        public string? ImgURL { get; set; } = string.Empty;

        public string AirlinePartner { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string? Address { get; set; } = string.Empty;

        public int? WardId { get; set; }

        public int? CityId { get; set; }
    }
}
