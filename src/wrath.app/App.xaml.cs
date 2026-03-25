using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        LogStartup("App constructor entered");

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

            LogStartup("Host built");
        }
        catch (Exception ex)
        {
            LogStartup($"App constructor failed: {ex}");
            throw;
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        LogStartup("OnLaunched start");

        if (_window is not null)
        {
            LogStartup("Existing window found; activating existing instance");
            _window.Activate();
            return;
        }

        try
        {
            LogStartup("Creating DI scope");
            _uiScope ??= _host.Services.CreateScope();
            var logger = _uiScope.ServiceProvider.GetRequiredService<ILogger<App>>();

            LogStartup("Before MainWindow creation");
            _window = _uiScope.ServiceProvider.GetRequiredService<MainWindow>();
            LogStartup("After MainWindow creation");

            _window.Activate();
            LogStartup("After Activate()");

            _ = InitializeAsync(logger);
        }
        catch (Exception ex)
        {
            LogStartup($"OnLaunched failed: {ex}");

            try
            {
                _window = new Window
                {
                    Content = new TextBlock
                    {
                        Text = "Wrath startup failed. See %LocalAppData%\\wrath\\startup.log",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(24)
                    }
                };
                _window.Activate();
                LogStartup("Fallback window activated");
            }
            catch (Exception fallbackEx)
            {
                LogStartup($"Fallback window failed: {fallbackEx}");
                throw;
            }
        }
    }

    private async Task InitializeAsync(ILogger<App> logger)
    {
        LogStartup("Background initialization started");

        try
        {
            var initializer = _uiScope!.ServiceProvider.GetRequiredService<SqliteInitializer>();
            await initializer.InitializeAsync(CancellationToken.None);
            LogStartup("Database initialization completed");

            if (_window is MainWindow mainWindow)
            {
                var enqueued = mainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    LogStartup("MainWindow shell initialization started");
                    mainWindow.InitializeShell();
                    LogStartup("MainWindow shell initialization completed");
                });

                LogStartup($"MainWindow shell initialization enqueue result: {enqueued}");
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Background initialization failed.");
            LogStartup($"Background initialization failed: {ex}");
        }
    }

    private void HookGlobalExceptionHandlers()
    {
        UnhandledException += (_, e) =>
        {
            LogStartup($"WinUI UnhandledException: {e.Exception}");
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            LogStartup($"AppDomain UnhandledException: {e.ExceptionObject}");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            LogStartup($"TaskScheduler UnobservedTaskException: {e.Exception}");
            e.SetObserved();
        };
    }

    private void LogStartup(string message)
    {
        var line = $"[{DateTimeOffset.Now:O}] {message}{Environment.NewLine}";
        Debug.WriteLine(line);
        Console.Error.WriteLine(line);
        File.AppendAllText(_startupLogPath, line, Encoding.UTF8);
    }
}
