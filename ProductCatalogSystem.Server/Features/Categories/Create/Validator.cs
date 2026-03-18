using FastEndpoints;
using FluentValidation;

namespace ProductCatalogSystem.Server.Features.Categories.Create;

public sealed class Validator : Validator<CreateCategoryRequest>
{
    public Validator()
    {
        RuleFor(request => request.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(request => request.Status)
            .IsInEnum();
    }
}
