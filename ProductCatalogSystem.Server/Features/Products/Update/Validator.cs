using FastEndpoints;
using FluentValidation;

namespace ProductCatalogSystem.Server.Features.Products.Update;

public sealed class Validator : Validator<UpdateProductRequest>
{
    public Validator()
    {
        RuleFor(request => request.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .When(request => request.HasName)
            .WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(request => request.Price)
            .NotNull()
            .When(request => request.HasPrice)
            .WithMessage("Price is required when supplied.");

        RuleFor(request => request.Price!.Value)
            .GreaterThanOrEqualTo(0)
            .When(request => request.HasPrice && request.Price is not null);

        RuleFor(request => request.InventoryOnHand)
            .NotNull()
            .When(request => request.HasInventoryOnHand)
            .WithMessage("Inventory On Hand is required when supplied.");

        RuleFor(request => request.InventoryOnHand!.Value)
            .GreaterThanOrEqualTo(0)
            .When(request => request.HasInventoryOnHand && request.InventoryOnHand is not null);

        RuleFor(request => request.CategoryId)
            .NotNull()
            .When(request => request.HasCategoryId)
            .WithMessage("Category Id is required when supplied.");

        RuleFor(request => request.PrimaryImageUrl)
            .Must(BeAValidAbsoluteUrl)
            .When(request => request.HasPrimaryImageUrl && !string.IsNullOrWhiteSpace(request.PrimaryImageUrl))
            .WithMessage("Primary Image Url must be a valid absolute URL.")
            .MaximumLength(1000);

        RuleFor(request => request.InventoryReason)
            .MaximumLength(250)
            .When(request => request.HasInventoryReason);

        RuleFor(request => request.ChangedBy)
            .MaximumLength(100)
            .When(request => request.HasChangedBy);

        RuleFor(request => request.RowVersion)
            .Must(rowVersion => !string.IsNullOrWhiteSpace(rowVersion))
            .WithMessage("Row Version is required.");
    }

    private static bool BeAValidAbsoluteUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }
}
