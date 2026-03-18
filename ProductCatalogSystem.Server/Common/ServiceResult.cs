namespace ProductCatalogSystem.Server.Common;

public sealed record ServiceResult<T>(
    ResultStatus Status,
    T? Value = default,
    string? Message = null,
    Dictionary<string, string[]>? Errors = null);
