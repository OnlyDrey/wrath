using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Wrath.Application;
using Wrath.Domain;
using Wrath.Infrastructure;
using Wrath.Protocols.Abstractions;
using Wrath.Protocols.Rdp;
using Wrath.Protocols.Ssh;
using Wrath.Security;
using Wrath.UI;

namespace Wrath.App;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        InitializeComponent();
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "wrath", "wrath.db");
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

                services.AddWrathInfrastructure($"Data Source={dbPath}");
                services.AddSingleton<ICredentialVaultAdapter, WindowsVaultAdapter>();

                services.AddScoped<ConnectionProfileService>();
                services.AddScoped<SessionService>();

                services.AddSingleton<IProtocolProvider, RdpProtocolProvider>();
                services.AddSingleton<IProtocolProvider, SshProtocolProvider>();
                services.AddSingleton<IProtocolDispatcher, ProtocolDispatchService>();

                services.AddTransient<MainShellViewModel>();
                services.AddTransient<MainWindow>();
            }).Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        using var scope = _host.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<SqliteInitializer>();
        await initializer.InitializeAsync(CancellationToken.None);

        var window = scope.ServiceProvider.GetRequiredService<MainWindow>();
        window.Activate();
    }
}
