namespace ProductCatalogSystem.Server.Common;

public sealed record PagingOptions(string? Cursor, int PageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static PagingOptions Normalize(string? cursor, int pageSize)
    {
        var normalizedCursor = string.IsNullOrWhiteSpace(cursor) ? null : cursor.Trim();
        var normalizedPageSize = pageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize
        };

        return new PagingOptions(normalizedCursor, normalizedPageSize);
    }
}
