namespace ProductCatalogSystem.Server.Common;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    string? Cursor,
    string? NextCursor);
