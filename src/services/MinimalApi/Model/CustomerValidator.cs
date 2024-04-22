using FluentValidation;

namespace MinimalApi.Model;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(customer => customer.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(("Email is required"))
            .EmailAddress().WithMessage("Invalid email format");
    }
}
