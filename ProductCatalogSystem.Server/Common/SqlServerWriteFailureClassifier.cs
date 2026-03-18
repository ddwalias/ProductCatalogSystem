using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductCatalogSystem.Server.Features.Categories.Create;
using ProductCatalogSystem.Server.Features.Categories.Shared;
using ProductCatalogSystem.Server.Features.Products.Create;
using ProductCatalogSystem.Server.Features.Products.Shared;

namespace ProductCatalogSystem.Server.Common;

public static class SqlServerWriteFailureClassifier
{
    private const int DuplicateKeyError = 2601;
    private const int UniqueConstraintError = 2627;
    private const int InactiveCategoryTriggerError = 51001;
    private const int CategoryCycleTriggerError = 51002;

    public static ServiceResult<CategoryTreeItemDto>? ClassifyCategoryFailure(
        DbUpdateException exception)
    {
        if (TryGetSqlException(exception) is not { } sqlException)
        {
            return null;
        }

        if (sqlException.Number is DuplicateKeyError or UniqueConstraintError)
        {
            return new ServiceResult<CategoryTreeItemDto>(
                ResultStatus.ValidationFailed,
                Errors: new Dictionary<string, string[]>
                {
                    [nameof(CreateCategoryRequest.DisplayOrder)] = ["Display order must be unique within the same parent category."]
                });
        }

        if (sqlException.Number == CategoryCycleTriggerError)
        {
            return new ServiceResult<CategoryTreeItemDto>(
                ResultStatus.ValidationFailed,
                Errors: new Dictionary<string, string[]>
                {
                    [nameof(CreateCategoryRequest.ParentCategoryId)] = ["The selected parent category would create a cycle."]
                });
        }

        return null;
    }

    public static ServiceResult<ProductDetailDto>? ClassifyProductFailure(
        DbUpdateException exception)
    {
        if (TryGetSqlException(exception) is not { } sqlException)
        {
            return null;
        }

        if (sqlException.Number == InactiveCategoryTriggerError)
        {
            return new ServiceResult<ProductDetailDto>(
                ResultStatus.ValidationFailed,
                Errors: new Dictionary<string, string[]>
                {
                    [nameof(CreateProductRequest.CategoryId)] = ["Inactive categories cannot be used for product assignment."]
                });
        }

        return null;
    }

    private static SqlException? TryGetSqlException(Exception exception)
    {
        return exception switch
        {
            SqlException sqlException => sqlException,
            DbUpdateException dbUpdateException => TryGetSqlException(dbUpdateException.InnerException ?? exception),
            _ => exception.InnerException is null ? null : TryGetSqlException(exception.InnerException)
        };
    }
}
