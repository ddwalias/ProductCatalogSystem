using Microsoft.EntityFrameworkCore;
using ProductCatalogSystem.Server.Common;
using ProductCatalogSystem.Server.Data;
using ProductCatalogSystem.Server.Domain;
using ProductCatalogSystem.Server.Features.Categories.Create;
using ProductCatalogSystem.Server.Features.Categories.Update;

namespace ProductCatalogSystem.Server.Features.Categories.Shared;

public interface ICategoryWriter
{
    Task<ServiceResult<CategoryTreeItemDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken);

    Task<ServiceResult<CategoryTreeItemDto>> UpdateAsync(long id, UpdateCategoryRequest request, CancellationToken cancellationToken);
}

public sealed class CategoryWriter(
    CatalogDbContext dbContext,
    ILogger<CategoryWriter> logger) : ICategoryWriter
{
    private const int DisplayOrderScale = 8;

    public async Task<ServiceResult<CategoryTreeItemDto>> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedDisplayOrder = NormalizeDisplayOrder(request.DisplayOrder);
        var validationErrors = await ValidateCategoryRequestAsync(request.ParentCategoryId, normalizedDisplayOrder, null, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return new ServiceResult<CategoryTreeItemDto>(ResultStatus.ValidationFailed, Errors: validationErrors);
        }

        var category = new Domain.Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            ParentCategoryId = request.ParentCategoryId,
            Status = request.Status,
            DisplayOrder = normalizedDisplayOrder
        };

        dbContext.Categories.Add(category);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (SqlServerWriteFailureClassifier.ClassifyCategoryFailure(exception) is { } classifiedFailure)
        {
            return classifiedFailure;
        }

        logger.LogInformation(
            "Category created {CategoryId} {CategoryName} {ParentCategoryId}",
            category.Id,
            category.Name,
            category.ParentCategoryId);

        return new ServiceResult<CategoryTreeItemDto>(
            ResultStatus.Success,
            new CategoryTreeItemDto(
                category.Id,
                category.ParentCategoryId,
                category.Name,
                category.Description,
                category.Status,
                category.DisplayOrder,
                category.CreatedAtUtc,
                category.UpdatedAtUtc,
                RowVersionConverter.Encode(category.RowVersion),
                []));
    }

    public async Task<ServiceResult<CategoryTreeItemDto>> UpdateAsync(
        long id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (category is null)
        {
            return new ServiceResult<CategoryTreeItemDto>(ResultStatus.NotFound, Message: $"Category {id} was not found.");
        }

        if (!RowVersionConverter.Matches(request.RowVersion, category.RowVersion))
        {
            return new ServiceResult<CategoryTreeItemDto>(ResultStatus.Conflict, Message: "The category has changed since it was last loaded.");
        }

        var updatedName = request.HasName ? request.Name!.Trim() : category.Name;
        var updatedDescription = request.HasDescription ? request.Description?.Trim() : category.Description;
        var updatedParentCategoryId = request.HasParentCategoryId ? request.ParentCategoryId : category.ParentCategoryId;
        var updatedStatus = request.HasStatus ? request.Status!.Value : category.Status;
        var updatedDisplayOrder = request.HasDisplayOrder
            ? NormalizeDisplayOrder(request.DisplayOrder!.Value)
            : category.DisplayOrder;

        var validationErrors = await ValidateCategoryRequestAsync(updatedParentCategoryId, updatedDisplayOrder, id, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return new ServiceResult<CategoryTreeItemDto>(ResultStatus.ValidationFailed, Errors: validationErrors);
        }

        category.Name = updatedName;
        category.Description = updatedDescription;
        category.ParentCategoryId = updatedParentCategoryId;
        category.Status = updatedStatus;
        category.DisplayOrder = updatedDisplayOrder;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ServiceResult<CategoryTreeItemDto>(ResultStatus.Conflict, Message: "The category was updated by another request.");
        }
        catch (DbUpdateException exception) when (SqlServerWriteFailureClassifier.ClassifyCategoryFailure(exception) is { } classifiedFailure)
        {
            return classifiedFailure;
        }

        logger.LogInformation(
            "Category updated {CategoryId} {CategoryName} {ParentCategoryId}",
            category.Id,
            category.Name,
            category.ParentCategoryId);

        return new ServiceResult<CategoryTreeItemDto>(
            ResultStatus.Success,
            new CategoryTreeItemDto(
                category.Id,
                category.ParentCategoryId,
                category.Name,
                category.Description,
                category.Status,
                category.DisplayOrder,
                category.CreatedAtUtc,
                category.UpdatedAtUtc,
                RowVersionConverter.Encode(category.RowVersion),
                []));
    }

    private async Task<Dictionary<string, string[]>> ValidateCategoryRequestAsync(
        long? parentCategoryId,
        decimal displayOrder,
        long? categoryId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (displayOrder < 0m)
        {
            errors[nameof(CreateCategoryRequest.DisplayOrder)] = ["Display order must be non-negative."];
        }

        if (parentCategoryId == categoryId && categoryId is not null)
        {
            errors[nameof(CreateCategoryRequest.ParentCategoryId)] = ["A category cannot be its own parent."];
            return errors;
        }

        if (parentCategoryId is not null)
        {
            var parent = await dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(category => category.Id == parentCategoryId, cancellationToken);

            if (parent is null)
            {
                errors[nameof(CreateCategoryRequest.ParentCategoryId)] = ["The specified parent category does not exist."];
                return errors;
            }

            if (categoryId is not null && await WouldCreateCycleAsync(categoryId.Value, parentCategoryId.Value, cancellationToken))
            {
                errors[nameof(CreateCategoryRequest.ParentCategoryId)] = ["The selected parent category would create a cycle."];
                return errors;
            }
        }

        var siblingConflict = await dbContext.Categories
            .AsNoTracking()
            .AnyAsync(category =>
                category.ParentCategoryId == parentCategoryId &&
                category.DisplayOrder == displayOrder &&
                (!categoryId.HasValue || category.Id != categoryId.Value), cancellationToken);

        if (siblingConflict)
        {
            errors[nameof(CreateCategoryRequest.DisplayOrder)] = ["Display order must be unique within the same parent category."];
        }

        return errors;
    }

    private static decimal NormalizeDisplayOrder(decimal displayOrder)
        => decimal.Round(displayOrder, DisplayOrderScale, MidpointRounding.AwayFromZero);

    private async Task<bool> WouldCreateCycleAsync(long categoryId, long proposedParentId, CancellationToken cancellationToken)
    {
        long? currentParentId = proposedParentId;

        while (currentParentId is not null)
        {
            if (currentParentId == categoryId)
            {
                return true;
            }

            currentParentId = await dbContext.Categories
                .Where(category => category.Id == currentParentId)
                .Select(category => category.ParentCategoryId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return false;
    }
}
