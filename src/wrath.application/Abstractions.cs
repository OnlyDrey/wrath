using Wrath.Domain;

namespace Wrath.Application;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed record ConnectionProfileRequest(
    string Name,
    ProtocolType Protocol,
    string Host,
    int Port,
    string? Username,
    string? GroupPath,
    IReadOnlyCollection<string> Tags);

public sealed record OperationResult(bool Succeeded, string Message, string? Error = null)
{
    public static OperationResult Success(string message) => new(true, message);
    public static OperationResult Failure(string message, string? error = null) => new(false, message, error);
}

public sealed record OperationResult<T>(bool Succeeded, T? Value, string Message, string? Error = null)
{
    public static OperationResult<T> Success(T value, string message) => new(true, value, message);
    public static OperationResult<T> Failure(string message, string? error = null) => new(false, default, message, error);
}
