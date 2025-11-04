using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Application.Users.Common
{
    public record UpdateUserRequest(
    string Fullname,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    Gender Gender,
    string Address,
    int WardId);
}
