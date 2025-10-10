namespace CabinCrewCareer.Infrastructure.Security
{
    public class JwtSettings
    {
        public const string Section = "JwtSettings";

        public string Secret { get; set; } = null!;
        public int TokenExpirationInMinutes { get; init; }
        public int RefreshTokenExpirationInMinutes { get; init; }
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
    }
}
