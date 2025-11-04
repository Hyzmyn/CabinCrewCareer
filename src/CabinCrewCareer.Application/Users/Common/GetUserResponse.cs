namespace CabinCrewCareer.Application.Users.Common
{
    public class GetUserResponse
    {
        public int UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string DateOfBirth { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public string? ImgURL { get; set; } = string.Empty;

        public string AirlinePartner { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
