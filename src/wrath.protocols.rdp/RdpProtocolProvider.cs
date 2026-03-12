using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Wrath.Domain;
using Wrath.Protocols.Abstractions;

namespace Wrath.Protocols.Rdp;

public sealed class RdpProtocolProvider : IProtocolProvider
{
    private readonly ILogger<RdpProtocolProvider> _logger;

    public RdpProtocolProvider(ILogger<RdpProtocolProvider> logger) => _logger = logger;

    public ProtocolType Protocol => ProtocolType.Rdp;

    public Task<LaunchResult> LaunchAsync(ConnectionProfile profile, CancellationToken ct)
    {
        var args = $"/v:{profile.Host}:{profile.Port}";
        var psi = new ProcessStartInfo("mstsc.exe", args)
        {
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            _logger.LogInformation("Launching RDP process. Host={Host} Port={Port}", profile.Host, profile.Port);
            Process.Start(psi);
            return Task.FromResult(new LaunchResult(true, "RDP launch triggered."));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RDP launch failed. Host={Host} Port={Port}", profile.Host, profile.Port);
            return Task.FromResult(new LaunchResult(false, "RDP launch failed.", ex.Message));
        }
    }
}
