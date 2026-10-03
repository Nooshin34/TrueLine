namespace TrueLine.Application.Services;

public enum ServiceError
{
    None,
    NotFound,
    BadRequest,
    Unauthorized,
    Conflict,
    Forbidden,
    StorageFailed,
}

public class ServiceResult
{
    public ServiceError Error { get; init; } = ServiceError.None;

    public string? Message { get; init; }

    public bool Succeeded => Error == ServiceError.None;

    public static ServiceResult Ok() => new();

    public static ServiceResult Fail(ServiceError error, string? message = null) =>
        new() { Error = error, Message = message };
}

public sealed class ServiceResult<T> : ServiceResult
{
    public T? Value { get; init; }

    public static ServiceResult<T> Ok(T value) => new() { Value = value };

    public static new ServiceResult<T> Fail(ServiceError error, string? message = null) =>
        new() { Error = error, Message = message };
}
