using Microsoft.Extensions.DependencyInjection;

namespace Wrath.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddWrathApplication(this IServiceCollection services)
    {
        services.AddScoped<ConnectionProfileService>();
        services.AddScoped<SessionService>();
        return services;
    }
}
