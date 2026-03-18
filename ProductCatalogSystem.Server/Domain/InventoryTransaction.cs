namespace ProductCatalogSystem.Server.Domain;

public sealed class InventoryTransaction
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public InventoryChangeType ChangeType { get; set; }

    public int Delta { get; set; }

    public int BeforeQty { get; set; }

    public int AfterQty { get; set; }

    public string? Reason { get; set; }

    public string? ChangedBy { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Product Product { get; set; } = null!;
}
