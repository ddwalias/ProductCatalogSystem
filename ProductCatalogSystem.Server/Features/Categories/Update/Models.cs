using System.Text.Json.Serialization;
using ProductCatalogSystem.Server.Domain;

namespace ProductCatalogSystem.Server.Features.Categories.Update;

public sealed class UpdateCategoryRequest
{
    private string? name;
    private string? description;
    private long? parentCategoryId;
    private CategoryStatus? status;
    private decimal? displayOrder;

    [JsonIgnore]
    public bool HasName { get; private set; }

    public string? Name
    {
        get => name;
        set
        {
            HasName = true;
            name = value;
        }
    }

    [JsonIgnore]
    public bool HasDescription { get; private set; }

    public string? Description
    {
        get => description;
        set
        {
            HasDescription = true;
            description = value;
        }
    }

    [JsonIgnore]
    public bool HasParentCategoryId { get; private set; }

    public long? ParentCategoryId
    {
        get => parentCategoryId;
        set
        {
            HasParentCategoryId = true;
            parentCategoryId = value;
        }
    }

    [JsonIgnore]
    public bool HasStatus { get; private set; }

    public CategoryStatus? Status
    {
        get => status;
        set
        {
            HasStatus = true;
            status = value;
        }
    }

    [JsonIgnore]
    public bool HasDisplayOrder { get; private set; }

    public decimal? DisplayOrder
    {
        get => displayOrder;
        set
        {
            HasDisplayOrder = true;
            displayOrder = value;
        }
    }

    public string RowVersion { get; set; } = string.Empty;
}
