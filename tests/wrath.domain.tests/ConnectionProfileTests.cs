using Wrath.Domain;

namespace Wrath.Domain.Tests;

public class ConnectionProfileTests
{
    [Fact]
    public void Constructor_ValidatesName()
    {
        Assert.Throws<DomainValidationException>(() =>
            new ConnectionProfile(Guid.NewGuid(), "a", ProtocolType.Rdp, "host", 3389, null, null));
    }

    [Fact]
    public void Constructor_ValidatesPort()
    {
        Assert.Throws<DomainValidationException>(() =>
            new ConnectionProfile(Guid.NewGuid(), "valid name", ProtocolType.Ssh, "host", 70000, null, null));
    }

    [Fact]
    public void Tags_AreNormalized()
    {
        var tag = new Tag("  Prod  ");
        Assert.Equal("prod", tag.Value);
    }

    [Fact]
    public void ConnectionProfile_Supports_Multiple_Deduplicated_Tags()
    {
        var profile = new ConnectionProfile(
            Guid.NewGuid(),
            "profile",
            ProtocolType.Ssh,
            "host",
            22,
            "user",
            null,
            [new Tag("ops"), new Tag("prod"), new Tag("ops")]);

        Assert.Equal(2, profile.Tags.Count);
        Assert.Contains(profile.Tags, t => t.Value == "ops");
        Assert.Contains(profile.Tags, t => t.Value == "prod");
    }

    [Fact]
    public void SessionRecord_Requires_Host_And_Tracks_Result()
    {
        var record = new SessionRecord(
            Guid.NewGuid(),
            ProtocolType.Rdp,
            "server01",
            SessionEventType.LaunchFailed,
            succeeded: false,
            "failed",
            "timeout");

        Assert.Equal("server01", record.Host);
        Assert.False(record.Succeeded);
        Assert.Equal("timeout", record.ErrorMessage);
    }
}
