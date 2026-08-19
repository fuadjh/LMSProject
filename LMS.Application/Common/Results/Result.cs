namespace Application.Common.Results;

public class Result : IResult
{
    protected Result(
        bool isSuccess,
        IReadOnlyCollection<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public IReadOnlyCollection<Error> Errors { get; }

    public static Result Success()
    {
        return new Result(
            true,
            Array.Empty<Error>());
    }

    public static Result Invalid(params Error[] errors)
    {
        return new Result(
            false,
            NormalizeErrors(errors));
    }

    public static Result Invalid(IEnumerable<Error> errors)
    {
        return new Result(
            false,
            NormalizeErrors(errors));
    }

    public static Result Invalid(string description)
    {
        return Invalid(
            Error.Validation(
                "validation",
                description));
    }

    public static Result Invalid(
        string code,
        string description)
    {
        return Invalid(
            Error.Validation(code, description));
    }

    public static Result Invalid(IEnumerable<string> errors)
    {
        return Invalid(
            ConvertMessages(
                errors,
                ErrorType.Validation));
    }

    public static Result Failure(string description)
    {
        return Failure(
            "operation.failed",
            description);
    }

    public static Result Failure(
        string code,
        string description)
    {
        return new Result(
            false,
            new[]
            {
                Error.Failure(code, description)
            });
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(
            false,
            ConvertMessages(
                errors,
                ErrorType.Failure));
    }

    public static Result Unauthorized(
        string code,
        string description)
    {
        return new Result(
            false,
            new[]
            {
                Error.Unauthorized(code, description)
            });
    }

    public static Result Forbidden(
        string code,
        string description)
    {
        return new Result(
            false,
            new[]
            {
                Error.Forbidden(code, description)
            });
    }

    public static Result Conflict(
        string code,
        string description)
    {
        return new Result(
            false,
            new[]
            {
                Error.Conflict(code, description)
            });
    }

    public static Result NotFound(
        string code,
        string description)
    {
        return new Result(
            false,
            new[]
            {
                Error.NotFound(code, description)
            });
    }

    protected static IReadOnlyCollection<Error> NormalizeErrors(
        IEnumerable<Error>? errors)
    {
        var normalized = errors?
            .Where(error => error is not null)
            .Distinct()
            .ToArray() ?? Array.Empty<Error>();

        return normalized.Length > 0
            ? normalized
            : new[]
            {
                Error.Failure(
                    "operation.failed",
                    "عملیات با خطا مواجه شد.")
            };
    }

    protected static IReadOnlyCollection<Error> ConvertMessages(
        IEnumerable<string>? messages,
        ErrorType errorType)
    {
        var values = messages?
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Select(message => message.Trim())
            .Distinct()
            .ToArray() ?? Array.Empty<string>();

        if (values.Length == 0)
        {
            values =
            [
                "عملیات با خطا مواجه شد."
            ];
        }

        return values
            .Select((message, index) =>
                errorType == ErrorType.Validation
                    ? Error.Validation(
                        $"validation.{index + 1}",
                        message)
                    : Error.Failure(
                        $"operation.{index + 1}",
                        message))
            .ToArray();
    }
}

public sealed class Result<T> : Result
{
    private Result(
        T? value,
        bool isSuccess,
        IReadOnlyCollection<Error> errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(
            value,
            true,
            Array.Empty<Error>());
    }

    public static new Result<T> Invalid(params Error[] errors)
    {
        return new Result<T>(
            default,
            false,
            NormalizeErrors(errors));
    }

    public static new Result<T> Invalid(IEnumerable<Error> errors)
    {
        return new Result<T>(
            default,
            false,
            NormalizeErrors(errors));
    }

    public static new Result<T> Invalid(string description)
    {
        return Invalid(
            Error.Validation(
                "validation",
                description));
    }

    public static new Result<T> Invalid(
        string code,
        string description)
    {
        return Invalid(
            Error.Validation(code, description));
    }

    public static new Result<T> Invalid(
        IEnumerable<string> errors)
    {
        return new Result<T>(
            default,
            false,
            ConvertMessages(
                errors,
                ErrorType.Validation));
    }

    public static new Result<T> Failure(string description)
    {
        return Failure(
            "operation.failed",
            description);
    }

    public static new Result<T> Failure(
        string code,
        string description)
    {
        return new Result<T>(
            default,
            false,
            new[]
            {
                Error.Failure(code, description)
            });
    }

    public static new Result<T> Failure(
        IEnumerable<string> errors)
    {
        return new Result<T>(
            default,
            false,
            ConvertMessages(
                errors,
                ErrorType.Failure));
    }

    public static new Result<T> Unauthorized(
        string code,
        string description)
    {
        return new Result<T>(
            default,
            false,
            new[]
            {
                Error.Unauthorized(code, description)
            });
    }

    public static new Result<T> Forbidden(
        string code,
        string description)
    {
        return new Result<T>(
            default,
            false,
            new[]
            {
                Error.Forbidden(code, description)
            });
    }

    public static new Result<T> Conflict(
        string code,
        string description)
    {
        return new Result<T>(
            default,
            false,
            new[]
            {
                Error.Conflict(code, description)
            });
    }

    public static new Result<T> NotFound(
        string code,
        string description)
    {
        return new Result<T>(
            default,
            false,
            new[]
            {
                Error.NotFound(code, description)
            });
    }
}