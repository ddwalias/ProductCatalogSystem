using System.Text.Json.Nodes;

namespace ProductCatalogSystem.Server.Common;

public static class JsonObjectSerializer
{
    public static string? Serialize(JsonObject? value)
    {
        if (value is null || value.Count == 0)
        {
            return null;
        }

        return value.ToJsonString();
    }

    public static JsonObject? Deserialize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return JsonNode.Parse(value) as JsonObject;
    }
}
