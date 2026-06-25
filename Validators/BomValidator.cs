using FluentValidation;
using mini_mes_be.DTOs.Bom;

namespace mini_mes_be.Validators;

public class CreateBomItemRequestValidator : AbstractValidator<CreateBomItemRequest>
{
    public CreateBomItemRequestValidator()
    {
        RuleFor(x => x.material_id)
            .GreaterThan(0).WithMessage("Material ID must be a positive integer.");

        RuleFor(x => x.quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.unit)
            .NotEmpty().WithMessage("Unit of measure is required.")
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");

        RuleFor(x => x.notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.");

        RuleFor(x => x.sort_order)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be a non-negative integer.");
    }
}

public class UpdateBomItemRequestValidator : AbstractValidator<UpdateBomItemRequest>
{
    public UpdateBomItemRequestValidator()
    {
        RuleFor(x => x.material_id)
            .GreaterThan(0).WithMessage("Material ID must be a positive integer.");

        RuleFor(x => x.quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.unit)
            .NotEmpty().WithMessage("Unit of measure is required.")
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");

        RuleFor(x => x.notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.");

        RuleFor(x => x.sort_order)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be a non-negative integer.");
    }
}

public class SetBomRequestValidator : AbstractValidator<SetBomRequest>
{
    public SetBomRequestValidator()
    {
        RuleFor(x => x.items)
            .NotNull().WithMessage("BOM items list is required.");

        RuleForEach(x => x.items).ChildRules(item =>
        {
            item.RuleFor(x => x.material_id)
                .GreaterThan(0).WithMessage("Material ID must be a positive integer.");

            item.RuleFor(x => x.quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            item.RuleFor(x => x.unit)
                .NotEmpty().WithMessage("Unit of measure is required.")
                .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");

            item.RuleFor(x => x.notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.");

            item.RuleFor(x => x.sort_order)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order must be a non-negative integer.");
        });
    }
}
