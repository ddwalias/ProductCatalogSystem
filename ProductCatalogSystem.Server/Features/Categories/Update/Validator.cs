using FastEndpoints;
using FluentValidation;

namespace ProductCatalogSystem.Server.Features.Categories.Update;

public sealed class Validator : Validator<UpdateCategoryRequest>
{
    public Validator()
    {
        Include(new Create.Validator());

        RuleFor(request => request.RowVersion)
            .Must(rowVersion => !string.IsNullOrWhiteSpace(rowVersion))
            .WithMessage("Row Version is required.");
    }
}
