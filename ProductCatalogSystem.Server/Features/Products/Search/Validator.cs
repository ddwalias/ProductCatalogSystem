using FastEndpoints;
using FluentValidation;
using ProductCatalogSystem.Server.Features.Products.Shared;

namespace ProductCatalogSystem.Server.Features.Products.Search;

public sealed class Validator : Validator<ProductAutocompleteQuery>
{
    public Validator()
    {
        RuleFor(request => request.Query)
            .Must(query => !string.IsNullOrWhiteSpace(query))
            .WithMessage("Search query is required.");

        RuleFor(request => request.Limit)
            .InclusiveBetween(1, 20);
    }
}
