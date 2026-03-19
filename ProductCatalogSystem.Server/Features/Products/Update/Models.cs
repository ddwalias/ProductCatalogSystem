using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ProductCatalogSystem.Server.Features.Products.Update;

public sealed class UpdateProductRequest
{
    private string? _name;
    private string? _description;
    private decimal? _price;
    private int? _inventoryOnHand;
    private long? _categoryId;
    private string? _primaryImageUrl;
    private JsonObject? _customAttributes;

    [JsonIgnore]
    public bool HasName { get; private set; }

    public string? Name
    {
        get => _name;
        set
        {
            HasName = true;
            _name = value;
        }
    }

    [JsonIgnore]
    public bool HasDescription { get; private set; }

    public string? Description
    {
        get => _description;
        set
        {
            HasDescription = true;
            _description = value;
        }
    }

    [JsonIgnore]
    public bool HasPrice { get; private set; }

    public decimal? Price
    {
        get => _price;
        set
        {
            HasPrice = true;
            _price = value;
        }
    }

    [JsonIgnore]
    public bool HasInventoryOnHand { get; private set; }

    public int? InventoryOnHand
    {
        get => _inventoryOnHand;
        set
        {
            HasInventoryOnHand = true;
            _inventoryOnHand = value;
        }
    }

    [JsonIgnore]
    public bool HasCategoryId { get; private set; }

    public long? CategoryId
    {
        get => _categoryId;
        set
        {
            HasCategoryId = true;
            _categoryId = value;
        }
    }

    [JsonIgnore]
    public bool HasPrimaryImageUrl { get; private set; }

    public string? PrimaryImageUrl
    {
        get => _primaryImageUrl;
        set
        {
            HasPrimaryImageUrl = true;
            _primaryImageUrl = value;
        }
    }

    [JsonIgnore]
    public bool HasCustomAttributes { get; private set; }

    public JsonObject? CustomAttributes
    {
        get => _customAttributes;
        set
        {
            HasCustomAttributes = true;
            _customAttributes = value;
        }
    }

    public string RowVersion { get; set; } = string.Empty;
}
