namespace Application.Common.Results;

public class Result : IResult
{
    protected Result(bool isSuccess, IReadOnlyCollection<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public IReadOnlyCollection<Error> Errors { get; }

    public static Result Success() => new(true, Array.Empty<Error>());

    public static Result Invalid(params Error[] errors) => new(false, errors);
    public static Result Invalid(IEnumerable<Error> errors) => new(false, errors.ToArray());

    public static Result Failure(string code, string description) =>
        new(false, new[] { Error.Failure(code, description) });

    public static Result Unauthorized(string code, string description) =>
        new(false, new[] { Error.Unauthorized(code, description) });

    public static Result Forbidden(string code, string description) =>
        new(false, new[] { Error.Forbidden(code, description) });

    public static Result Conflict(string code, string description) =>
        new(false, new[] { Error.Conflict(code, description) });

    public static Result NotFound(string code, string description) =>
        new(false, new[] { Error.NotFound(code, description) });
}

public sealed class Result<T> : Result
{
    private Result(T? value, bool isSuccess, IReadOnlyCollection<Error> errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(value, true, Array.Empty<Error>());

    public static new Result<T> Invalid(params Error[] errors) => new(default, false, errors);
    public static Result<T> Invalid(IEnumerable<Error> errors) => new(default, false, errors.ToArray());

    public static new Result<T> Failure(string code, string description) =>
        new(default, false, new[] { Error.Failure(code, description) });

    public static new Result<T> Unauthorized(string code, string description) =>
        new(default, false, new[] { Error.Unauthorized(code, description) });

    public static new Result<T> Forbidden(string code, string description) =>
        new(default, false, new[] { Error.Forbidden(code, description) });

    public static new Result<T> Conflict(string code, string description) =>
        new(default, false, new[] { Error.Conflict(code, description) });

    public static new Result<T> NotFound(string code, string description) =>
        new(default, false, new[] { Error.NotFound(code, description) });
}