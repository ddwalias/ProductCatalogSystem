namespace ProductCatalogSystem.Server.Common;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    string? Cursor,
    string? NextCursor);
