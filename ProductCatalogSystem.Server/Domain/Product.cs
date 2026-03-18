namespace ProductCatalogSystem.Server.Domain;

public sealed class Product : SoftDeletableEntity
{
    public long Id { get; set; }

    public long CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int InventoryOnHand { get; set; }

    public string? PrimaryImageUrl { get; set; }

    public string? CustomAttributesJson { get; set; }

    public int VersionNumber { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public Category Category { get; set; } = null!;

    public List<InventoryTransaction> InventoryTransactions { get; set; } = [];
}
