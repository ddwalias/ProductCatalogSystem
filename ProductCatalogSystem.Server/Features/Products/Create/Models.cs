using System.Text.Json.Nodes;

namespace ProductCatalogSystem.Server.Features.Products.Create;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int InventoryOnHand { get; set; }

    public long CategoryId { get; set; }

    public string? PrimaryImageUrl { get; set; }

    public JsonObject? CustomAttributes { get; set; }
}
