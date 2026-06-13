using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Rapsodia.Blue.Infrastructure.Configuration;

public static class AuthConfig
{
    public static IServiceCollection AddAuthConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}