using Microsoft.Extensions.DependencyInjection;
using Wrath.Domain;

namespace Wrath.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddWrathInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(new SqliteOptions { ConnectionString = connectionString });
        services.AddSingleton<SqliteInitializer>();
        services.AddScoped<IConnectionProfileRepository, ConnectionProfileRepository>();
        services.AddScoped<ISessionHistoryRepository, SessionHistoryRepository>();
        return services;
    }
}
