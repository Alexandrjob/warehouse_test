using FluentValidation;
using warehouse_api.Models;

namespace warehouse_api.Validators;

public class UnitValidator : AbstractValidator<Unit>
{
    public UnitValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}
