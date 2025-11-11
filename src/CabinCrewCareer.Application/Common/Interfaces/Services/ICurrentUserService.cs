using System.Security.Claims;

namespace CabinCrewCareer.Application.Common.Interfaces.Services
{
    public interface ICurrentUserService
    {
        public string? CurrentPrincipal { get; }

        public string? Email { get; }

        public string? UserName { get; }

        public ClaimsPrincipal GetCurrentPrincipalFromToken(string token);
    }
}
