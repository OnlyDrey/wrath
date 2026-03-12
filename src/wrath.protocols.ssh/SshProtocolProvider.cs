using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Wrath.Domain;
using Wrath.Protocols.Abstractions;

namespace Wrath.Protocols.Ssh;

public sealed class SshProtocolProvider : IProtocolProvider
{
    private readonly ILogger<SshProtocolProvider> _logger;

    public SshProtocolProvider(ILogger<SshProtocolProvider> logger) => _logger = logger;

    public ProtocolType Protocol => ProtocolType.Ssh;

    public Task<LaunchResult> LaunchAsync(ConnectionProfile profile, CancellationToken ct)
    {
        var userHost = string.IsNullOrWhiteSpace(profile.Username)
            ? profile.Host
            : $"{profile.Username}@{profile.Host}";

        var psi = new ProcessStartInfo("ssh.exe", $"-p {profile.Port} {userHost}")
        {
            UseShellExecute = false,
            CreateNoWindow = false
        };

        try
        {
            _logger.LogInformation("Launching SSH process. Host={Host} Port={Port}", profile.Host, profile.Port);
            Process.Start(psi);
            return Task.FromResult(new LaunchResult(true, "SSH launch triggered."));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SSH launch failed. Host={Host} Port={Port}", profile.Host, profile.Port);
            return Task.FromResult(new LaunchResult(false, "SSH launch failed.", ex.Message));
        }
    }
}
