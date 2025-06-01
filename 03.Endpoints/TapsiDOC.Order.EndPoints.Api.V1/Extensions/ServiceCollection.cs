using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TapsiDOC.Order.EndPoints.Api.V1.LegacyIntegration;

namespace TapsiDOC.Order.EndPoints.Api.V1.Extensions;
internal static class ApplicationDependencyRegistrator
{

    internal static IServiceCollection AddOIDCIdentity(
        this IServiceCollection services, IConfiguration configuration)
    {

        services
            .AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKeys = ApplicationTokens.Tokens.Values,
                    ValidIssuer = configuration["ValidIssuer"],
                    ValidAudiences = ApplicationTokens.Tokens.Keys
                };
            });
        return services;
    }
}
