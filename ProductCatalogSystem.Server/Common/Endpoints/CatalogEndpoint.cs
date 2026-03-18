using FastEndpoints;

namespace ProductCatalogSystem.Server.Common.Endpoints;

public abstract class CatalogEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
    where TRequest : notnull
{
    protected async Task<bool> TryHandleValidationFailuresAsync(CancellationToken cancellationToken)
    {
        if (!ValidationFailed)
        {
            return false;
        }

        await HttpContext.WriteValidationProblemAsync(
            ValidationFailures.ToErrorDictionary(),
            cancellationToken);

        return true;
    }

    protected async Task<bool> TryHandleServiceResultAsync(
        ServiceResult<TResponse> result,
        CancellationToken cancellationToken)
    {
        if (result.Status == ResultStatus.Success)
        {
            return false;
        }

        await HttpContext.WriteServiceResultAsync(result, cancellationToken);
        return true;
    }
}

public abstract class CatalogEndpointWithoutRequest<TResponse> : EndpointWithoutRequest<TResponse>
{
    protected async Task<bool> TryHandleServiceResultAsync(
        ServiceResult<TResponse> result,
        CancellationToken cancellationToken)
    {
        if (result.Status == ResultStatus.Success)
        {
            return false;
        }

        await HttpContext.WriteServiceResultAsync(result, cancellationToken);
        return true;
    }
}
