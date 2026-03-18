using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace ProductCatalogSystem.Server.Common;

public static class EndpointValidationExtensions
{
    public static Dictionary<string, string[]> ToErrorDictionary(
        this IEnumerable<ValidationFailure> failures)
    {
        return failures
            .Where(failure => failure is not null)
            .GroupBy(failure => ToContractFieldName(failure.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(failure => string.IsNullOrWhiteSpace(failure.ErrorMessage)
                        ? "The request is invalid."
                        : failure.ErrorMessage)
                    .Distinct()
                    .ToArray());
    }

    private static string ToContractFieldName(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return string.Empty;
        }

        return string.Join(
            '.',
            propertyName
                .Split('.')
                .Select(segment => segment.Length == 0
                    ? segment
                    : char.ToUpperInvariant(segment[0]) + segment[1..]));
    }

    public static Task WriteValidationProblemAsync(
        this HttpContext httpContext,
        IReadOnlyDictionary<string, string[]> errors,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return httpContext.Response.WriteAsJsonAsync(
            new HttpValidationProblemDetails(errors)
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest
            },
            cancellationToken);
    }

    public static Task WriteServiceResultAsync<T>(
        this HttpContext httpContext,
        ServiceResult<T> result,
        CancellationToken cancellationToken)
    {
        return result.Status switch
        {
            ResultStatus.NotFound => httpContext.WriteProblemAsync(
                StatusCodes.Status404NotFound,
                "Resource not found",
                result.Message,
                cancellationToken),
            ResultStatus.ValidationFailed => httpContext.WriteValidationProblemAsync(
                result.Errors ?? new Dictionary<string, string[]>(),
                cancellationToken),
            ResultStatus.Conflict => httpContext.WriteProblemAsync(
                StatusCodes.Status409Conflict,
                "Concurrency conflict",
                result.Message,
                cancellationToken),
            _ => httpContext.WriteProblemAsync(
                StatusCodes.Status500InternalServerError,
                "Unexpected service result",
                result.Message,
                cancellationToken)
        };
    }

    public static Task WriteProblemAsync(
        this HttpContext httpContext,
        int statusCode,
        string title,
        string? detail,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = statusCode;

        return httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Title = title,
                Detail = detail,
                Status = statusCode
            },
            cancellationToken);
    }
}
