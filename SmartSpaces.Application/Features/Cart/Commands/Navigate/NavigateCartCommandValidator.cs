using FluentValidation;

namespace SmartSpaces.Application.Features.Cart.Commands.Navigate;

public class NavigateCartCommandValidator : AbstractValidator<NavigateCartCommand>
{
    public NavigateCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El userId es requerido.");

        RuleFor(x => x.TargetZone)
            .NotEmpty().WithMessage("La zona destino es requerida.");
    }
}
