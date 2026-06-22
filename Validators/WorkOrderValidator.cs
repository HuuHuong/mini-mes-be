using FluentValidation;
using mini_mes_be.DTOs.WorkOrders;

namespace mini_mes_be.Validators;

public class WorkOrderProductRequestValidator : AbstractValidator<WorkOrderProductRequest>
{
    public WorkOrderProductRequestValidator()
    {
        RuleFor(x => x.product_id)
            .GreaterThan(0).WithMessage("Valid Product ID is required.");

        RuleFor(x => x.target_quantity)
            .GreaterThan(0).WithMessage("Target quantity must be greater than 0.");
    }
}

public class CreateWorkOrderRequestValidator : AbstractValidator<CreateWorkOrderRequest>
{
    public CreateWorkOrderRequestValidator()
    {
        RuleFor(x => x.products)
            .NotEmpty().WithMessage("At least one product is required.");

        RuleForEach(x => x.products)
            .SetValidator(new WorkOrderProductRequestValidator());

        RuleFor(x => x.machine_id)
            .GreaterThan(0).WithMessage("Valid Machine ID is required.");

        RuleFor(x => x.notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.");
    }
}

public class UpdateWorkOrderRequestValidator : AbstractValidator<UpdateWorkOrderRequest>
{
    public UpdateWorkOrderRequestValidator()
    {
        RuleFor(x => x.products)
            .NotEmpty().WithMessage("At least one product is required.");

        RuleForEach(x => x.products)
            .SetValidator(new WorkOrderProductRequestValidator());

        RuleFor(x => x.machine_id)
            .GreaterThan(0).WithMessage("Valid Machine ID is required.");

        RuleFor(x => x.notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.");
    }
}

public class RecordOutputRequestValidator : AbstractValidator<RecordOutputRequest>
{
    public RecordOutputRequestValidator()
    {
        RuleFor(x => x.product_id)
            .GreaterThan(0).WithMessage("Valid Product ID is required.");

        RuleFor(x => x.quantity)
            .GreaterThan(0).WithMessage("Recorded production quantity must be greater than 0.");

        RuleFor(x => x.defect_quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Defect quantity cannot be negative.")
            .LessThanOrEqualTo(x => x.quantity).WithMessage("Defect quantity cannot be greater than produced quantity.");
    }
}
