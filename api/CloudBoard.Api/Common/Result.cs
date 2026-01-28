namespace CloudBoard.Api.Common;

/// <summary>
/// Represents the outcome of an operation that can succeed or fail.
/// Use instead of throwing exceptions for expected failure cases.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public int StatusCode { get; }

    protected Result(bool isSuccess, string? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result Success() => new(true, null, 200);
    public static Result Failure(string error, int statusCode = 400) => new(false, error, statusCode);
    public static Result NotFound(string message = "Resource not found") => new(false, message, 404);
    public static Result Forbidden(string message = "Access denied") => new(false, message, 403);
    public static Result BadRequest(string message) => new(false, message, 400);
}

/// <summary>
/// Generic Result with a value payload for successful operations.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? error, int statusCode)
        : base(isSuccess, error, statusCode)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null, 200);
    public static new Result<T> Failure(string error, int statusCode = 400) => new(false, default, error, statusCode);
    public static new Result<T> NotFound(string message = "Resource not found") => new(false, default, message, 404);
    public static new Result<T> Forbidden(string message = "Access denied") => new(false, default, message, 403);
    public static new Result<T> BadRequest(string message) => new(false, default, message, 400);

    /// <summary>
    /// Implicit conversion from value to successful Result
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);
}
