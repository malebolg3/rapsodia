using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Rapsodia.Blue.Infrastructure.Configuration;

public static class RateLimitConfig
{
    public static IServiceCollection AddBlueRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("Global", config =>
            {
                config.PermitLimit = 100;
                config.Window = TimeSpan.FromMinutes(1);
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                config.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("Auth", config =>
            {
                config.PermitLimit = 5;
                config.Window = TimeSpan.FromMinutes(1);
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                config.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("Api", config =>
            {
                config.PermitLimit = 60;
                config.Window = TimeSpan.FromMinutes(1);
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                config.QueueLimit = 10;
            });

            options.AddFixedWindowLimiter("Health", config =>
            {
                config.PermitLimit = 30;
                config.Window = TimeSpan.FromMinutes(1);
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                config.QueueLimit = 0;
            });
        });

        return services;
    }
}