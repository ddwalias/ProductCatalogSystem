using FastEndpoints;
using FluentValidation;

namespace ProductCatalogSystem.Server.Features.Products.Create;

public sealed class Validator : Validator<CreateProductRequest>
{
    public Validator()
    {
        RuleFor(request => request.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(request => request.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.InventoryOnHand)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.PrimaryImageUrl)
            .Must(BeAValidAbsoluteUrl)
            .When(request => !string.IsNullOrWhiteSpace(request.PrimaryImageUrl))
            .WithMessage("Primary Image Url must be a valid absolute URL.")
            .MaximumLength(1000);
    }

    private static bool BeAValidAbsoluteUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }
}
