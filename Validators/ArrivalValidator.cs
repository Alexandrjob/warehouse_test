using FluentValidation;
using warehouse_api.Models;

namespace warehouse_api.Validators;

public class ArrivalValidator : AbstractValidator<Arrival>
{
    public ArrivalValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Resources);
    }
}
