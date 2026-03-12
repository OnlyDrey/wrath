using System.Diagnostics;
using Wrath.Application;
using Wrath.Domain;
using Wrath.Protocols.Abstractions;

namespace Wrath.Protocols.Rdp;

public sealed class RdpProtocolProvider : IProtocolProvider
{
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
            Process.Start(psi);
            return Task.FromResult(new LaunchResult(true, "RDP launch triggered."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new LaunchResult(false, $"RDP launch failed: {ex.Message}"));
        }
    }
}
