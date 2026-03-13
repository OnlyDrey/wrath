using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Wrath.Infrastructure;

namespace Wrath.App;

public partial class App : Microsoft.UI.Xaml.Application
{
    private readonly IHost _host;
    private readonly string _startupLogPath;
    private IServiceScope? _uiScope;
    private Window? _window;

    public App()
    {
        _startupLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "wrath", "startup.log");
        Directory.CreateDirectory(Path.GetDirectoryName(_startupLogPath)!);

        HookGlobalExceptionHandlers();
        WriteStartupLog("App constructor entered");

        try
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

            WriteStartupLog("Host built");
        }
        catch (Exception ex)
        {
            WriteStartupLog($"App constructor failed: {ex}");
            throw;
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        WriteStartupLog("OnLaunched entered");

        if (_window is not null)
        {
            WriteStartupLog("Existing MainWindow instance found; re-activating.");
            _window.Activate();
            return;
        }

        try
        {
            WriteStartupLog("Creating DI scope");
            _uiScope ??= _host.Services.CreateScope();
            var logger = _uiScope.ServiceProvider.GetRequiredService<ILogger<App>>();

            WriteStartupLog("Resolving MainWindow");
            _window = _uiScope.ServiceProvider.GetRequiredService<MainWindow>();
            WriteStartupLog("MainWindow resolved");

            _window.Activate();
            WriteStartupLog("MainWindow Activate() called");

            _ = InitializeAsync(logger);
        }
        catch (Exception ex)
        {
            WriteStartupLog($"Application startup failed: {ex}");
            throw;
        }
    }

    private async Task InitializeAsync(ILogger<App> logger)
    {
        WriteStartupLog("Background initialization started");

        try
        {
            var initializer = _uiScope!.ServiceProvider.GetRequiredService<SqliteInitializer>();
            await initializer.InitializeAsync(CancellationToken.None);
            WriteStartupLog("Database initialization completed");

            if (_window is MainWindow mainWindow)
            {
                var enqueued = mainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        WriteStartupLog("MainWindow shell initialization started");
                        mainWindow.InitializeShell();
                        WriteStartupLog("MainWindow shell initialization completed");
                    }
                    catch (Exception ex)
                    {
                        WriteStartupLog($"MainWindow shell initialization failed: {ex}");
                    }
                });

                WriteStartupLog($"MainWindow shell initialization enqueue result: {enqueued}");
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Background initialization failed.");
            WriteStartupLog($"Background initialization failed: {ex}");
        }
    }

    private void HookGlobalExceptionHandlers()
    {
        UnhandledException += (_, e) =>
        {
            WriteStartupLog($"WinUI UnhandledException: {e.Exception}");
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            WriteStartupLog($"AppDomain UnhandledException: {e.ExceptionObject}");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            WriteStartupLog($"TaskScheduler UnobservedTaskException: {e.Exception}");
            e.SetObserved();
        };
    }

    private void WriteStartupLog(string message)
    {
        var line = $"[{DateTimeOffset.Now:O}] {message}{Environment.NewLine}";
        File.AppendAllText(_startupLogPath, line, Encoding.UTF8);
    }
}
