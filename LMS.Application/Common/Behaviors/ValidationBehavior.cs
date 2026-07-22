using Application.Common.Results;
using FluentValidation;
using MediatR;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>

    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (!failures.Any())
            return await next();

        var errors = failures
            .Select(f => Results.Error.Validation(f.PropertyName, f.ErrorMessage))
            .ToArray();

        return CreateValidationResult(errors);
    }

    private static TResponse CreateValidationResult(Results.Error[] errors)
    {
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Invalid(errors);

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var method = typeof(TResponse).GetMethod(
                nameof(Result<object>.Invalid),
                BindingFlags.Public | BindingFlags.Static,
                new[] { typeof(IEnumerable<Results.Error>) });

            if (method is not null)
                return (TResponse)method.Invoke(null, new object[] { errors })!;
        }

        throw new InvalidOperationException($"ValidationBehavior cannot create result for {typeof(TResponse).Name}");
    }
}