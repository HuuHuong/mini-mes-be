using FluentValidation;
using mini_mes_be.DTOs.Machines;

namespace mini_mes_be.Validators;

public class CreateMachineRequestValidator : AbstractValidator<CreateMachineRequest>
{
    public CreateMachineRequestValidator()
    {
        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Machine name is required.")
            .MaximumLength(200).WithMessage("Machine name must not exceed 200 characters.");

        RuleFor(x => x.code)
            .NotEmpty().WithMessage("Machine code is required.")
            .MaximumLength(50).WithMessage("Machine code must not exceed 50 characters.");

        RuleFor(x => x.location)
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters.");
    }
}

public class UpdateMachineRequestValidator : AbstractValidator<UpdateMachineRequest>
{
    public UpdateMachineRequestValidator()
    {
        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Machine name is required.")
            .MaximumLength(200).WithMessage("Machine name must not exceed 200 characters.");

        RuleFor(x => x.code)
            .NotEmpty().WithMessage("Machine code is required.")
            .MaximumLength(50).WithMessage("Machine code must not exceed 50 characters.");

        RuleFor(x => x.location)
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters.");
    }
}
