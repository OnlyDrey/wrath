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
}
