using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Wrath.Infrastructure;

namespace Wrath.App;

public partial class App : Microsoft.UI.Xaml.Application
{
    private readonly IHost _host;
    private IServiceScope? _uiScope;
    private Window? _window;

    public App()
    {
        InitializeComponent();
        _host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
                logging.AddConsole();
            })
            .ConfigureServices(services =>
            {
                var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "wrath", "wrath.db");
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

                services.AddApplication();
                services.AddInfrastructure($"Data Source={dbPath}");
                services.AddProtocols();
                services.AddSecurity();
                services.AddTransient<MainWindow>();
            }).Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        _uiScope = _host.Services.CreateScope();
        var logger = _uiScope.ServiceProvider.GetRequiredService<ILogger<App>>();

        try
        {
            var initializer = _uiScope.ServiceProvider.GetRequiredService<SqliteInitializer>();
            await initializer.InitializeAsync(CancellationToken.None);

            _window = _uiScope.ServiceProvider.GetRequiredService<MainWindow>();
            _window.Activate();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Application startup failed.");
            throw;
        }
    }
}
