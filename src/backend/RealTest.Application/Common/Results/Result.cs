namespace RealTest.Application.Common.Results;

/// <summary>
/// Lightweight success/failure result for internal application-service flows that have more
/// than one expected outcome (e.g. refresh-token rotation). Use exceptions for outcomes that
/// should terminate the request pipeline; use Result where the caller needs to branch on *why*
/// an operation didn't succeed.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }

    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);

    public static Result Failure(string error) => new(false, error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    private Result(bool isSuccess, T? value, string error) : base(isSuccess, error)
    {
        _value = value;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);

    public static new Result<T> Failure(string error) => new(false, default, error);
}
