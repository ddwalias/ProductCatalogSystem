namespace ProductCatalogSystem.Server.Common;

public static class RowVersionConverter
{
    public static string Encode(byte[] rowVersion) => Convert.ToBase64String(rowVersion);

    public static bool Matches(string? encodedRowVersion, byte[] rowVersion)
    {
        if (string.IsNullOrWhiteSpace(encodedRowVersion))
        {
            return false;
        }

        try
        {
            return Convert.ToBase64String(rowVersion) == encodedRowVersion;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
