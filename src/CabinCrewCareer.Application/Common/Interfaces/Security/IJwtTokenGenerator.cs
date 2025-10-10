using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Interfaces.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateJwtToken(User user);
        string GenerateJwtRefreshToken(User user);
    }
}
