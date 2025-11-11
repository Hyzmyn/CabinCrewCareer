using CabinCrewCareer.Application.Common.Interfaces.Security;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CabinCrewCareer.Infrastructure.Security
{
    public class JwtTokenGenerator(
        IDateTimeProvider dateTimeProvider,
        IOptions<JwtSettings> jwtOptions) : IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        public string GenerateJwtRefreshToken(User user)
        {
            return GenerateToken(user, _jwtSettings.RefreshTokenExpirationInMinutes);
        }

        public string GenerateJwtToken(User user)
        {
            return GenerateToken(user, _jwtSettings.TokenExpirationInMinutes);
        }

        private string GenerateToken(User user, int expireInMinutes)
        {
            var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.RoleName),
            new Claim(ClaimTypes.UserData, user.UserName  ?? string.Empty),
            new Claim(ClaimTypes.PrimarySid, user.RoleId.ToString() ?? string.Empty)
        };

            var securityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: _dateTimeProvider.UtcNow.AddMinutes(expireInMinutes),
                claims: claims,
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }
    }
}
