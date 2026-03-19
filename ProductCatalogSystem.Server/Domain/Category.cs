namespace ProductCatalogSystem.Server.Domain;

public sealed class Category : AuditableEntity
{
    public long Id { get; set; }

    public long? ParentCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public CategoryStatus Status { get; set; }

    public decimal DisplayOrder { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public Category? ParentCategory { get; set; }

    public List<Category> Children { get; set; } = [];

    public List<Product> Products { get; set; } = [];
}
