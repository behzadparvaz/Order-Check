using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TapsiDOC.Order.EndPoints.Api.V1.LegacyIntegration
{
    public static class JwtSecurityKey
    {
        public static SymmetricSecurityKey Create(string secret)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }
    }
}
