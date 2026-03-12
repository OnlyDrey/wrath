namespace Wrath.Domain;

public enum ProtocolType
{
    Rdp = 1,
    Ssh = 2,
    Sftp = 3,
    Https = 4
}

public enum SessionEventType
{
    LaunchRequested = 1,
    LaunchSucceeded = 2,
    LaunchFailed = 3,
    Closed = 4
}
