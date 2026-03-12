using Microsoft.Extensions.DependencyInjection;

namespace Wrath.Security;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddWrathSecurity(this IServiceCollection services)
    {
        services.AddSingleton<ICredentialVaultAdapter, WindowsVaultAdapter>();
        return services;
    }
}
