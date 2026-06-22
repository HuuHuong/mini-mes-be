using FluentValidation;
using mini_mes_be.DTOs.QualityChecks;

namespace mini_mes_be.Validators;

public class CreateQualityCheckRequestValidator : AbstractValidator<CreateQualityCheckRequest>
{
    public CreateQualityCheckRequestValidator()
    {
        RuleFor(x => x.work_order_id)
            .GreaterThan(0).WithMessage("Valid Work Order ID is required.");

        RuleFor(x => x.inspected_quantity)
            .GreaterThan(0).WithMessage("Inspected quantity must be greater than 0.");

        RuleFor(x => x.passed_quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Passed quantity cannot be negative.");

        RuleFor(x => x.failed_quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Failed quantity cannot be negative.");

        RuleFor(x => x)
            .Must(x => x.passed_quantity + x.failed_quantity == x.inspected_quantity)
            .WithMessage("Inspected quantity must equal the sum of passed and failed quantities.");

        RuleFor(x => x.notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.");
    }
}
