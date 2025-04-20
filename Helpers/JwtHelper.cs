using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PainForGlory_Web.Helpers
{
    public static class JwtHelper
    {
        public static ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                return new ClaimsPrincipal(identity);
            }
            catch
            {
                return null;
            }
        }

    }
}
