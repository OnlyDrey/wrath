using Microsoft.Extensions.DependencyInjection;
using Wrath.Application;
using Wrath.Infrastructure;
using Wrath.Protocols.Abstractions;
using Wrath.Protocols.Rdp;
using Wrath.Protocols.Ssh;
using Wrath.Security;
using Wrath.UI;

namespace Wrath.App;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddWrathApplication();
        services.AddTransient<MainShellViewModel>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddWrathInfrastructure(connectionString);
        return services;
    }

    public static IServiceCollection AddProtocols(this IServiceCollection services)
    {
        services.AddSingleton<IProtocolProvider, RdpProtocolProvider>();
        services.AddSingleton<IProtocolProvider, SshProtocolProvider>();
        services.AddSingleton<IProtocolDispatcher, ProtocolDispatchService>();
        return services;
    }

    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddWrathSecurity();
        return services;
    }
}
