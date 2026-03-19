using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ProductCatalogSystem.Server.Domain;

namespace ProductCatalogSystem.Server.Data;

public static class DbInitializer
{
    private static readonly string[] FinishPalette =
    [
        "graphite",
        "linen",
        "sand",
        "forest",
        "cobalt",
        "ember",
        "mist",
        "bronze",
        "ivory"
    ];

    private static readonly string[] UsagePalette =
    [
        "focused desk setups",
        "hybrid team routines",
        "travel-heavy workflows",
        "creator stations",
        "quiet home offices",
        "studio corners",
        "daily carry kits",
        "shared meeting spaces",
        "compact apartments",
        "customer-facing counters"
    ];

    private static readonly string[] PersonaPalette =
    [
        "operations leads",
        "design teams",
        "remote operators",
        "content producers",
        "office managers",
        "analysts",
        "project coordinators",
        "travelers",
        "wellness-minded teams",
        "support staff"
    ];

    private static readonly CategorySeed[] CategorySeeds =
    [
        new(
            Name: "Electronics",
            Description: "Devices, accessories, and everyday tech.",
            ParentName: null,
            DisplayOrder: 10,
            Collection: "Vertex",
            HeroImageUrl: "https://images.unsplash.com/photo-1519389950473-47ba0277781c",
            Focus: "connected daily setups",
            ProductLabels:
            [
                "Control Hub",
                "Portable Power Station",
                "Charging Dock",
                "Travel Adapter",
                "Desktop Hub",
                "Signal Booster",
                "Compact Scanner",
                "Field Recorder",
                "Device Stand",
                "Pocket Projector"
            ]),
        new(
            Name: "Audio",
            Description: "Headphones, speakers, and audio interfaces.",
            ParentName: "Electronics",
            DisplayOrder: 10,
            Collection: "Auraline",
            HeroImageUrl: "https://images.unsplash.com/photo-1505740420928-5e560c06d30e",
            Focus: "clear listening and studio-ready playback",
            ProductLabels:
            [
                "Monitor Headphones",
                "Travel Earbuds",
                "Bookshelf Speakers",
                "USB Audio Interface",
                "Podcast Microphone",
                "Conference Speaker",
                "Desktop DAC",
                "Sound Bar",
                "Bass Module",
                "Studio Mixer"
            ]),
        new(
            Name: "Peripherals",
            Description: "Keyboards, mice, and desk accessories.",
            ParentName: "Electronics",
            DisplayOrder: 20,
            Collection: "Keystone",
            HeroImageUrl: "https://images.unsplash.com/photo-1511467687858-23d96c32e4ae",
            Focus: "faster desk workflows",
            ProductLabels:
            [
                "Mechanical Keyboard",
                "Wireless Mouse",
                "Precision Trackpad",
                "Docking Station",
                "Monitor Arm",
                "Laptop Stand",
                "USB Switch",
                "Webcam Light",
                "Desk Mat",
                "Macro Keypad"
            ]),
        new(
            Name: "Smart Home",
            Description: "Automation, sensors, and connected home control.",
            ParentName: "Electronics",
            DisplayOrder: 30,
            Collection: "Nexa",
            HeroImageUrl: "https://images.unsplash.com/photo-1558002038-1055907df827",
            Focus: "automated rooms and ambient control",
            ProductLabels:
            [
                "Room Sensor",
                "Smart Plug",
                "Climate Hub",
                "Video Doorbell",
                "Indoor Camera",
                "Scene Controller",
                "Air Quality Monitor",
                "Leak Detector",
                "Presence Sensor",
                "Smart Thermostat"
            ]),
        new(
            Name: "Cameras",
            Description: "Capture tools for creators, teams, and live spaces.",
            ParentName: "Electronics",
            DisplayOrder: 40,
            Collection: "Framecraft",
            HeroImageUrl: "https://images.unsplash.com/photo-1516035069371-29a1b244cc32",
            Focus: "fast capture and creator workflows",
            ProductLabels:
            [
                "Mirrorless Camera",
                "Streaming Camera",
                "Prime Lens",
                "Travel Tripod",
                "Capture Card",
                "Light Panel",
                "Field Monitor",
                "Camera Backpack",
                "Battery Grip",
                "Wireless Flash"
            ]),
        new(
            Name: "Office",
            Description: "Workflow and office productivity products.",
            ParentName: null,
            DisplayOrder: 20,
            Collection: "Northline",
            HeroImageUrl: "https://images.unsplash.com/photo-1497366754035-f200968a6e72",
            Focus: "calm and efficient workdays",
            ProductLabels:
            [
                "Focus Timer",
                "Document Scanner",
                "Desktop Whiteboard",
                "Cable Caddy",
                "Task Light",
                "Standing Mat",
                "Phone Stand",
                "Note Organizer",
                "Reference Shelf",
                "Planner Folio"
            ]),
        new(
            Name: "Furniture",
            Description: "Desks, seating, and support pieces for focused work.",
            ParentName: "Office",
            DisplayOrder: 10,
            Collection: "Atlas",
            HeroImageUrl: "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85",
            Focus: "comfortable long-session ergonomics",
            ProductLabels:
            [
                "Standing Desk",
                "Ergonomic Chair",
                "Credenza",
                "Side Table",
                "Meeting Stool",
                "Monitor Shelf",
                "Mobile Pedestal",
                "Focus Booth Table",
                "Foot Rest",
                "Storage Bench"
            ]),
        new(
            Name: "Lighting",
            Description: "Task, ambient, and scene lighting for workspaces.",
            ParentName: "Office",
            DisplayOrder: 20,
            Collection: "Luma",
            HeroImageUrl: "https://images.unsplash.com/photo-1513694203232-719a280e022f",
            Focus: "layered lighting moods",
            ProductLabels:
            [
                "Desk Lamp",
                "Floor Lamp",
                "Bias Light Bar",
                "Pendant Light",
                "Accent Lamp",
                "Wall Sconce",
                "Reading Light",
                "Studio Lamp",
                "Ambient Strip",
                "Clamp Lamp"
            ]),
        new(
            Name: "Organization",
            Description: "Storage, sorting, and analog workflow tools.",
            ParentName: "Office",
            DisplayOrder: 30,
            Collection: "Orderly",
            HeroImageUrl: "https://images.unsplash.com/photo-1484480974693-6ca0a78fb36b",
            Focus: "cleaner desks and clearer routines",
            ProductLabels:
            [
                "Archive Box",
                "Drawer Tray",
                "Vertical File Rack",
                "Stacking Shelf",
                "Label Kit",
                "Supply Caddy",
                "Cable Sleeve Pack",
                "Magazine Holder",
                "Desktop Catchall",
                "Desktop Locker"
            ]),
        new(
            Name: "Lifestyle",
            Description: "Daily carry and comfort products around modern routines.",
            ParentName: null,
            DisplayOrder: 30,
            Collection: "Drift",
            HeroImageUrl: "https://images.unsplash.com/photo-1512436991641-6745cdb1723f",
            Focus: "lighter daily rituals",
            ProductLabels:
            [
                "Weekender Tote",
                "Daily Organizer",
                "Travel Wallet",
                "Portable Blanket",
                "Packing Cube Set",
                "Crossbody Pack",
                "City Umbrella",
                "Tech Pouch",
                "Everyday Sling",
                "Bottle Sling"
            ])
    ];

    public static async Task EnsureCatalogDatabaseReadyAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync();
            return;
        }

        await dbContext.Database.EnsureCreatedAsync();
    }

    public static void SeedCatalog(CatalogDbContext dbContext)
    {
        var logger = dbContext.GetService<ILoggerFactory>()
            .CreateLogger("CatalogDbInitializer");
        var categoriesByName = dbContext.Categories
            .ToList()
            .ToDictionary(category => category.Name, StringComparer.OrdinalIgnoreCase);

        var addedCategoryCount = 0;

        foreach (var seed in CategorySeeds)
        {
            if (categoriesByName.ContainsKey(seed.Name))
            {
                continue;
            }

            Category? parentCategory = null;
            if (seed.ParentName is not null &&
                !categoriesByName.TryGetValue(seed.ParentName, out parentCategory))
            {
                throw new InvalidOperationException($"Seed category parent '{seed.ParentName}' was not found for '{seed.Name}'.");
            }

            var category = new Category
            {
                Name = seed.Name,
                Description = seed.Description,
                ParentCategory = parentCategory,
                DisplayOrder = seed.DisplayOrder,
                Status = CategoryStatus.Active
            };

            dbContext.Categories.Add(category);
            categoriesByName[seed.Name] = category;
            addedCategoryCount++;
        }

        if (addedCategoryCount > 0)
        {
            dbContext.SaveChanges();
        }

        var existingProductNames = dbContext.Products
            .IgnoreQueryFilters()
            .Select(product => product.Name)
            .ToList()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var productsToAdd = BuildProductSeeds(categoriesByName)
            .Where(seed => !existingProductNames.Contains(seed.Name))
            .Select(seed => new Product
            {
                CategoryId = categoriesByName[seed.CategoryName].Id,
                Name = seed.Name,
                Description = seed.Description,
                Price = seed.Price,
                InventoryOnHand = seed.InventoryOnHand,
                PrimaryImageUrl = seed.PrimaryImageUrl,
                CustomAttributesJson = seed.CustomAttributesJson,
                VersionNumber = 1
            })
            .ToArray();

        if (productsToAdd.Length > 0)
        {
            dbContext.Products.AddRange(productsToAdd);
            dbContext.UseInventoryAudit("Seeded extended catalog inventory", "seed");
            dbContext.SaveChanges();
        }

        var totalCategories = dbContext.Categories.Count();
        var totalProducts = dbContext.Products.Count();

        logger.LogInformation(
            "Catalog seed ensured with {CategoryCount} categories and {ProductCount} products. Added {AddedCategoryCount} categories and {AddedProductCount} products in this run.",
            totalCategories,
            totalProducts,
            addedCategoryCount,
            productsToAdd.Length);
    }

    public static async Task SeedCatalogAsync(CatalogDbContext dbContext, CancellationToken cancellationToken)
    {
        var logger = dbContext.GetService<ILoggerFactory>()
            .CreateLogger("CatalogDbInitializer");
        var categoriesByName = (await dbContext.Categories
                .ToListAsync(cancellationToken))
            .ToDictionary(category => category.Name, StringComparer.OrdinalIgnoreCase);

        var addedCategoryCount = 0;

        foreach (var seed in CategorySeeds)
        {
            if (categoriesByName.ContainsKey(seed.Name))
            {
                continue;
            }

            Category? parentCategory = null;
            if (seed.ParentName is not null &&
                !categoriesByName.TryGetValue(seed.ParentName, out parentCategory))
            {
                throw new InvalidOperationException($"Seed category parent '{seed.ParentName}' was not found for '{seed.Name}'.");
            }

            var category = new Category
            {
                Name = seed.Name,
                Description = seed.Description,
                ParentCategory = parentCategory,
                DisplayOrder = seed.DisplayOrder,
                Status = CategoryStatus.Active
            };

            dbContext.Categories.Add(category);
            categoriesByName[seed.Name] = category;
            addedCategoryCount++;
        }

        if (addedCategoryCount > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var existingProductNames = (await dbContext.Products
                .IgnoreQueryFilters()
                .Select(product => product.Name)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var productsToAdd = BuildProductSeeds(categoriesByName)
            .Where(seed => !existingProductNames.Contains(seed.Name))
            .Select(seed => new Product
            {
                CategoryId = categoriesByName[seed.CategoryName].Id,
                Name = seed.Name,
                Description = seed.Description,
                Price = seed.Price,
                InventoryOnHand = seed.InventoryOnHand,
                PrimaryImageUrl = seed.PrimaryImageUrl,
                CustomAttributesJson = seed.CustomAttributesJson,
                VersionNumber = 1
            })
            .ToArray();

        if (productsToAdd.Length > 0)
        {
            dbContext.Products.AddRange(productsToAdd);
            dbContext.UseInventoryAudit("Seeded extended catalog inventory", "seed");
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var totalCategories = await dbContext.Categories.CountAsync(cancellationToken);
        var totalProducts = await dbContext.Products.CountAsync(cancellationToken);

        logger.LogInformation(
            "Catalog seed ensured with {CategoryCount} categories and {ProductCount} products. Added {AddedCategoryCount} categories and {AddedProductCount} products in this run.",
            totalCategories,
            totalProducts,
            addedCategoryCount,
            productsToAdd.Length);
    }

    private static IEnumerable<ProductSeed> BuildProductSeeds(IReadOnlyDictionary<string, Category> categoriesByName)
    {
        foreach (var categorySeed in CategorySeeds)
        {
            _ = categoriesByName[categorySeed.Name];

            for (var index = 0; index < categorySeed.ProductLabels.Length; index++)
            {
                var label = categorySeed.ProductLabels[index];
                var name = $"{categorySeed.Collection} {label}";
                var price = decimal.Round(49m + (categorySeed.DisplayOrder * 1.8m) + (index * 17.5m), 2);
                var inventory = 8 + ((categorySeed.DisplayOrder + index * 7) % 43);

                yield return new ProductSeed(
                    CategoryName: categorySeed.Name,
                    Name: name,
                    Description: BuildProductDescription(categorySeed, label, index),
                    Price: price,
                    InventoryOnHand: inventory,
                    PrimaryImageUrl: BuildProductImageUrl(name),
                    CustomAttributesJson: BuildAttributesJson(categorySeed, index));
            }
        }
    }

    private static string BuildProductDescription(CategorySeed categorySeed, string label, int index)
    {
        var usage = UsagePalette[(categorySeed.DisplayOrder + index) % UsagePalette.Length];
        var persona = PersonaPalette[(index + categorySeed.Collection.Length) % PersonaPalette.Length];

        return $"{categorySeed.Collection} {label} is built for {categorySeed.Focus}, giving {persona} a cleaner fit for {usage}.";
    }

    private static string BuildProductImageUrl(string productName)
        => $"https://picsum.photos/seed/{Uri.EscapeDataString(productName.ToLowerInvariant().Replace(' ', '-'))}/1200/900";

    private static string BuildAttributesJson(CategorySeed categorySeed, int index)
    {
        var label = categorySeed.ProductLabels[index];
        var attributes = new JsonObject
        {
            ["collection"] = categorySeed.Collection,
            ["edition"] = index + 1,
            ["finish"] = FinishPalette[index % FinishPalette.Length],
            ["bundleReady"] = index % 2 == 0,
            ["profile"] = categorySeed.Focus,
            ["warrantyYears"] = 1 + (index % 3),
            ["category"] = categorySeed.Name,
            ["label"] = label,
            ["usage"] = UsagePalette[(categorySeed.DisplayOrder + index) % UsagePalette.Length],
            ["persona"] = PersonaPalette[(index + categorySeed.Collection.Length) % PersonaPalette.Length],
            ["sku"] = $"{categorySeed.Collection[..Math.Min(3, categorySeed.Collection.Length)].ToUpperInvariant()}-{categorySeed.DisplayOrder:00}-{index + 1:00}"
        };

        return attributes.ToJsonString();
    }

    private sealed record CategorySeed(
        string Name,
        string Description,
        string? ParentName,
        int DisplayOrder,
        string Collection,
        string HeroImageUrl,
        string Focus,
        string[] ProductLabels);

    private sealed record ProductSeed(
        string CategoryName,
        string Name,
        string Description,
        decimal Price,
        int InventoryOnHand,
        string PrimaryImageUrl,
        string CustomAttributesJson);
}
