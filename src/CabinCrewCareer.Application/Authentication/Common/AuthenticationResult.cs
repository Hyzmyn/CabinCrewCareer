namespace CabinCrewCareer.Application.Authentication.Common
{
    public class AuthenticationResult(string accessToken, string refreshToken)
    {
        public string AccessToken { get; private set; } = accessToken;

        public string RefreshToken { get; private set; } = refreshToken;
    }
}
