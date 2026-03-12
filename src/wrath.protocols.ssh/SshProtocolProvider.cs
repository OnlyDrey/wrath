using System.Diagnostics;
using Wrath.Application;
using Wrath.Domain;
using Wrath.Protocols.Abstractions;

namespace Wrath.Protocols.Ssh;

public sealed class SshProtocolProvider : IProtocolProvider
{
    public ProtocolType Protocol => ProtocolType.Ssh;

    public Task<LaunchResult> LaunchAsync(ConnectionProfile profile, CancellationToken ct)
    {
        // MVP safe extension point: process handoff only, terminal surface to be implemented in wrath.ui.
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
            Process.Start(psi);
            return Task.FromResult(new LaunchResult(true, "SSH launch triggered."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new LaunchResult(false, $"SSH launch failed: {ex.Message}"));
        }
    }
}
