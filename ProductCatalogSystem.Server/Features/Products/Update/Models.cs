using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ProductCatalogSystem.Server.Features.Products.Update;

public sealed class UpdateProductRequest
{
    private string? name;
    private string? description;
    private decimal? price;
    private int? inventoryOnHand;
    private long? categoryId;
    private string? primaryImageUrl;
    private JsonObject? customAttributes;
    private string? inventoryReason;
    private string? changedBy;

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
    public bool HasPrice { get; private set; }

    public decimal? Price
    {
        get => price;
        set
        {
            HasPrice = true;
            price = value;
        }
    }

    [JsonIgnore]
    public bool HasInventoryOnHand { get; private set; }

    public int? InventoryOnHand
    {
        get => inventoryOnHand;
        set
        {
            HasInventoryOnHand = true;
            inventoryOnHand = value;
        }
    }

    [JsonIgnore]
    public bool HasCategoryId { get; private set; }

    public long? CategoryId
    {
        get => categoryId;
        set
        {
            HasCategoryId = true;
            categoryId = value;
        }
    }

    [JsonIgnore]
    public bool HasPrimaryImageUrl { get; private set; }

    public string? PrimaryImageUrl
    {
        get => primaryImageUrl;
        set
        {
            HasPrimaryImageUrl = true;
            primaryImageUrl = value;
        }
    }

    [JsonIgnore]
    public bool HasCustomAttributes { get; private set; }

    public JsonObject? CustomAttributes
    {
        get => customAttributes;
        set
        {
            HasCustomAttributes = true;
            customAttributes = value;
        }
    }

    [JsonIgnore]
    public bool HasInventoryReason { get; private set; }

    public string? InventoryReason
    {
        get => inventoryReason;
        set
        {
            HasInventoryReason = true;
            inventoryReason = value;
        }
    }

    [JsonIgnore]
    public bool HasChangedBy { get; private set; }

    public string? ChangedBy
    {
        get => changedBy;
        set
        {
            HasChangedBy = true;
            changedBy = value;
        }
    }

    public string RowVersion { get; set; } = string.Empty;
}
