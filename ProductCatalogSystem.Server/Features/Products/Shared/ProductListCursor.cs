using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ProductCatalogSystem.Server.Features.Products.Shared;

internal static class ProductListCursor
{
    public static string BuildSignature(ProductListQuery query)
    {
        return string.Join(
            '|',
            NormalizeNullable(query.Query),
            query.CategoryId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            query.PriceFrom?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            query.PriceTo?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            query.NormalizedSortBy,
            query.SortDescending ? "desc" : "asc");
    }

    public static string EncodeDatabaseCursor(ProductListQuery query, ProductListItemDto item)
    {
        var payload = new ProductListCursorPayload
        {
            Mode = "db",
            Signature = BuildSignature(query),
            SortBy = query.NormalizedSortBy,
            SortDescending = query.SortDescending,
            Id = item.Id,
            Name = item.Name,
            Price = item.Price,
            InventoryOnHand = item.InventoryOnHand,
            UpdatedAtUtc = item.UpdatedAtUtc
        };

        return Encode(payload);
    }

    public static bool TryDecodeDatabaseCursor(string? encodedCursor, ProductListQuery query, out DatabaseCursor? cursor)
    {
        cursor = null;
        if (!TryDecode(encodedCursor, out var payload) ||
            payload.Mode != "db" ||
            payload.Signature != BuildSignature(query) ||
            payload.SortBy != query.NormalizedSortBy ||
            payload.SortDescending != query.SortDescending)
        {
            return false;
        }

        cursor = new DatabaseCursor(
            payload.SortBy,
            payload.SortDescending,
            payload.Id,
            payload.Name,
            payload.Price,
            payload.InventoryOnHand,
            payload.UpdatedAtUtc);
        return true;
    }

    public static string EncodeSearchCursor(ProductListQuery query, double score, long id)
    {
        var payload = new ProductListCursorPayload
        {
            Mode = "search",
            Signature = BuildSignature(query),
            Id = id,
            Score = score
        };

        return Encode(payload);
    }

    public static bool TryDecodeSearchCursor(string? encodedCursor, ProductListQuery query, out SearchCursor? cursor)
    {
        cursor = null;
        if (!TryDecode(encodedCursor, out var payload) ||
            payload.Mode != "search" ||
            payload.Signature != BuildSignature(query) ||
            payload.Score is null)
        {
            return false;
        }

        cursor = new SearchCursor(payload.Score.Value, payload.Id);
        return true;
    }

    private static string NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();

    private static string Encode(ProductListCursorPayload payload)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static bool TryDecode(string? encodedCursor, out ProductListCursorPayload payload)
    {
        payload = default!;
        if (string.IsNullOrWhiteSpace(encodedCursor))
        {
            return false;
        }

        try
        {
            var incoming = encodedCursor.Replace('-', '+').Replace('_', '/');
            var padded = incoming.PadRight(incoming.Length + (4 - incoming.Length % 4) % 4, '=');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            var decoded = JsonSerializer.Deserialize<ProductListCursorPayload>(json);
            if (decoded is null)
            {
                return false;
            }

            payload = decoded;
            return true;
        }
        catch
        {
            return false;
        }
    }

    internal sealed record DatabaseCursor(
        string SortBy,
        bool SortDescending,
        long Id,
        string? Name,
        decimal? Price,
        int? InventoryOnHand,
        DateTime? UpdatedAtUtc);

    internal sealed record SearchCursor(double Score, long Id);

    private sealed class ProductListCursorPayload
    {
        public string Mode { get; set; } = string.Empty;

        public string Signature { get; set; } = string.Empty;

        public string SortBy { get; set; } = string.Empty;

        public bool SortDescending { get; set; }

        public long Id { get; set; }

        public string? Name { get; set; }

        public decimal? Price { get; set; }

        public int? InventoryOnHand { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public double? Score { get; set; }
    }
}
