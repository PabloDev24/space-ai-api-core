using FluentValidation;

namespace SmartSpaces.Application.Features.Access.Commands.CreatePoint;

public class CreateAccessPointCommandValidator : AbstractValidator<CreateAccessPointCommand>
{
    public CreateAccessPointCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del punto de acceso es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Building)
            .NotEmpty().WithMessage("El edificio o ubicación es obligatorio.")
            .MaximumLength(150).WithMessage("El edificio no puede exceder 150 caracteres.");

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("El identificador del lector es obligatorio.")
            .MaximumLength(100).WithMessage("El identificador del lector no puede exceder 100 caracteres.");

        RuleFor(x => x.Status)
            .Must(AccessPointCatalog.IsValidStatus)
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage($"El estado debe ser uno de: {string.Join(", ", AccessPointCatalog.Statuses)}.");

        RuleFor(x => x.NetworkPingMs)
            .InclusiveBetween(0, 10000)
            .When(x => x.NetworkPingMs.HasValue)
            .WithMessage("El ping de red debe estar entre 0 y 10000 ms.");
    }
}
