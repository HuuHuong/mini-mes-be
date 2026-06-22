using FluentValidation;
using mini_mes_be.DTOs.Products;

namespace mini_mes_be.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.sku)
            .NotEmpty().WithMessage("Product SKU is required.")
            .MaximumLength(100).WithMessage("Product SKU must not exceed 100 characters.");

        RuleFor(x => x.unit)
            .NotEmpty().WithMessage("Unit of measure is required.")
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");

        RuleFor(x => x.description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.sku)
            .NotEmpty().WithMessage("Product SKU is required.")
            .MaximumLength(100).WithMessage("Product SKU must not exceed 100 characters.");

        RuleFor(x => x.unit)
            .NotEmpty().WithMessage("Unit of measure is required.")
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");

        RuleFor(x => x.description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}
