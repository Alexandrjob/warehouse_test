using FluentValidation;
using warehouse_api.Models;

namespace warehouse_api.Validators;

public class ResourceValidator : AbstractValidator<Resource>
{
    public ResourceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
