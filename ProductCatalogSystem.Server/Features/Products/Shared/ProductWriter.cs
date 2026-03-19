using Microsoft.EntityFrameworkCore;
using ProductCatalogSystem.Server.Common;
using ProductCatalogSystem.Server.Data;
using ProductCatalogSystem.Server.Domain;
using ProductCatalogSystem.Server.Features.Products.Create;
using ProductCatalogSystem.Server.Features.Products.Update;

namespace ProductCatalogSystem.Server.Features.Products.Shared;

public interface IProductWriter
{
    Task<ServiceResult<ProductDetailDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);

    Task<ServiceResult<ProductDetailDto>> UpdateAsync(long id, UpdateProductRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
}

public sealed class ProductWriter(
    CatalogDbContext dbContext,
    ILogger<ProductWriter> logger,
    IProductReader productReader) : IProductWriter
{
    public async Task<ServiceResult<ProductDetailDto>> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var validationErrors = await ValidateProductRequestAsync(request.CategoryId, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return new ServiceResult<ProductDetailDto>(ResultStatus.ValidationFailed, Errors: validationErrors);
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var product = new Product
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                Price = request.Price,
                InventoryOnHand = request.InventoryOnHand,
                CategoryId = request.CategoryId,
                PrimaryImageUrl = request.PrimaryImageUrl?.Trim(),
                CustomAttributesJson = JsonObjectSerializer.Serialize(request.CustomAttributes),
                VersionNumber = 1
            };

            dbContext.Products.Add(product);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (SqlServerWriteFailureClassifier.ClassifyProductFailure(exception) is { } classifiedFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return classifiedFailure;
            }

            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation(
                "Product created {ProductId} {CategoryId} {InventoryOnHand}",
                product.Id,
                product.CategoryId,
                product.InventoryOnHand);

            var dto = await productReader.GetByIdAsync(product.Id, null, cancellationToken);
            return new ServiceResult<ProductDetailDto>(ResultStatus.Success, dto!);
        });
    }

    public async Task<ServiceResult<ProductDetailDto>> UpdateAsync(
        long id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (request.HasCategoryId)
        {
            var validationErrors = await ValidateProductRequestAsync(request.CategoryId!.Value, cancellationToken);
            if (validationErrors.Count > 0)
            {
                return new ServiceResult<ProductDetailDto>(ResultStatus.ValidationFailed, Errors: validationErrors);
            }
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var product = await dbContext.Products
                .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

            if (product is null)
            {
                return new ServiceResult<ProductDetailDto>(ResultStatus.NotFound, Message: $"Product {id} was not found.");
            }

            if (!RowVersionConverter.Matches(request.RowVersion, product.RowVersion))
            {
                return new ServiceResult<ProductDetailDto>(ResultStatus.Conflict, Message: "The product has changed since it was last loaded.");
            }

            var updatedName = request.HasName ? request.Name!.Trim() : product.Name;
            var updatedDescription = request.HasDescription ? request.Description?.Trim() : product.Description;
            var updatedPrice = request.HasPrice ? request.Price!.Value : product.Price;
            var updatedInventoryOnHand = request.HasInventoryOnHand ? request.InventoryOnHand!.Value : product.InventoryOnHand;
            var updatedCategoryId = request.HasCategoryId ? request.CategoryId!.Value : product.CategoryId;
            var updatedPrimaryImageUrl = request.HasPrimaryImageUrl ? request.PrimaryImageUrl?.Trim() : product.PrimaryImageUrl;
            var updatedCustomAttributesJson = request.HasCustomAttributes
                ? JsonObjectSerializer.Serialize(request.CustomAttributes)
                : product.CustomAttributesJson;

            var hasChanges =
                !string.Equals(product.Name, updatedName, StringComparison.Ordinal) ||
                !string.Equals(product.Description, updatedDescription, StringComparison.Ordinal) ||
                product.Price != updatedPrice ||
                product.InventoryOnHand != updatedInventoryOnHand ||
                product.CategoryId != updatedCategoryId ||
                !string.Equals(product.PrimaryImageUrl, updatedPrimaryImageUrl, StringComparison.Ordinal) ||
                !string.Equals(product.CustomAttributesJson, updatedCustomAttributesJson, StringComparison.Ordinal);

            if (!hasChanges)
            {
                var unchangedDto = await productReader.GetByIdAsync(product.Id, null, cancellationToken);
                return new ServiceResult<ProductDetailDto>(ResultStatus.Success, unchangedDto!);
            }

            product.Name = updatedName;
            product.Description = updatedDescription;
            product.Price = updatedPrice;
            product.InventoryOnHand = updatedInventoryOnHand;
            product.CategoryId = updatedCategoryId;
            product.PrimaryImageUrl = updatedPrimaryImageUrl;
            product.CustomAttributesJson = updatedCustomAttributesJson;
            product.VersionNumber += 1;

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new ServiceResult<ProductDetailDto>(ResultStatus.Conflict, Message: "The product was updated by another request.");
            }
            catch (DbUpdateException exception) when (SqlServerWriteFailureClassifier.ClassifyProductFailure(exception) is { } classifiedFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return classifiedFailure;
            }

            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation(
                "Product updated {ProductId} {CategoryId} {VersionNumber} {InventoryOnHand}",
                product.Id,
                product.CategoryId,
                product.VersionNumber,
                product.InventoryOnHand);

            var dto = await productReader.GetByIdAsync(product.Id, null, cancellationToken);
            return new ServiceResult<ProductDetailDto>(ResultStatus.Success, dto!);
        });
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var product = await dbContext.Products
                .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

            if (product is null)
            {
                return false;
            }

            product.VersionNumber += 1;
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Product deleted {ProductId}", product.Id);
            return true;
        });
    }

    private async Task<Dictionary<string, string[]>> ValidateProductRequestAsync(long categoryId, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        var category = await dbContext.Categories
            .AsNoTracking()
            .Where(current => current.Id == categoryId)
            .Select(current => new { current.Id, current.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
        {
            errors[nameof(CreateProductRequest.CategoryId)] = ["The specified category does not exist."];
            return errors;
        }

        if (category.Status != CategoryStatus.Active)
        {
            errors[nameof(CreateProductRequest.CategoryId)] = ["Inactive categories cannot be used for product assignment."];
        }

        return errors;
    }
}
