namespace Application.Common.Results;

public sealed class Result<T> : IResult
{
    private Result(
        bool isSuccess,
        T? value,
        IReadOnlyCollection<Error> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T? Value { get; }

    public IReadOnlyCollection<Error> Errors { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(
            true,
            value,
            Array.Empty<Error>());
    }

    public static Result<T> Failure(params Error[] errors)
    {
        return new Result<T>(
            false,
            default,
            NormalizeErrors(errors));
    }

    public static Result<T> Failure(
        IEnumerable<Error> errors)
    {
        return new Result<T>(
            false,
            default,
            NormalizeErrors(errors));
    }

    public static Result<T> Failure(
        string code,
        string description)
    {
        return Failure(
            Error.Failure(code, description));
    }

    public static Result<T> Invalid(params Error[] errors)
    {
        return Failure(errors);
    }

    public static Result<T> Invalid(
        string code,
        string description)
    {
        return Failure(
            Error.Validation(code, description));
    }

    public static Result<T> NotFound(
        string code,
        string description)
    {
        return Failure(
            Error.NotFound(code, description));
    }

    public static Result<T> Conflict(
        string code,
        string description)
    {
        return Failure(
            Error.Conflict(code, description));
    }

    public static Result<T> Unauthorized(
        string code,
        string description)
    {
        return Failure(
            Error.Unauthorized(code, description));
    }

    public static Result<T> Forbidden(
        string code,
        string description)
    {
        return Failure(
            Error.Forbidden(code, description));
    }

    private static IReadOnlyCollection<Error> NormalizeErrors(
        IEnumerable<Error>? errors)
    {
        if (errors is null)
        {
            return Array.Empty<Error>();
        }

        return errors
            .Where(error => error is not null)
            .Distinct()
            .ToArray();
    }
}