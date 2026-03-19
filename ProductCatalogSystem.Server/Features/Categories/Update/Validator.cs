using FastEndpoints;
using FluentValidation;

namespace ProductCatalogSystem.Server.Features.Categories.Update;

public sealed class Validator : Validator<UpdateCategoryRequest>
{
    public Validator()
    {
        RuleFor(request => request.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .When(request => request.HasName)
            .WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(request => request.Status)
            .NotNull()
            .When(request => request.HasStatus)
            .WithMessage("Status is required when supplied.");

        RuleFor(request => request.Status!.Value)
            .IsInEnum()
            .When(request => request.HasStatus && request.Status is not null);

        RuleFor(request => request.DisplayOrder)
            .NotNull()
            .When(request => request.HasDisplayOrder)
            .WithMessage("Display Order is required when supplied.");

        RuleFor(request => request.DisplayOrder!.Value)
            .GreaterThanOrEqualTo(0)
            .When(request => request.HasDisplayOrder && request.DisplayOrder is not null);

        RuleFor(request => request.RowVersion)
            .Must(rowVersion => !string.IsNullOrWhiteSpace(rowVersion))
            .WithMessage("Row Version is required.");
    }
}
