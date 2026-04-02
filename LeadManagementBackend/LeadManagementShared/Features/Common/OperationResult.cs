namespace LeadManagementSystem.Features.Common;

public sealed record OperationResult(bool Success, string Message)
{
    public static OperationResult Ok(string message) => new(true, message);

    public static OperationResult Fail(string message) => new(false, message);
}

public sealed record OperationResult<T>(bool Success, string Message, T? Value)
{
    public static OperationResult<T> Ok(T value, string message) => new(true, message, value);

    public static OperationResult<T> Fail(string message) => new(false, message, default);
}
