using FastEndpoints;
using FluentValidation;
using ProductCatalogSystem.Server.Features.Products.Create;

namespace ProductCatalogSystem.Server.Features.Products.Update;

public sealed class Validator : Validator<UpdateProductRequest>
{
    public Validator()
    {
        Include(new Create.Validator());

        RuleFor(request => request.RowVersion)
            .Must(rowVersion => !string.IsNullOrWhiteSpace(rowVersion))
            .WithMessage("Row Version is required.");
    }
}
