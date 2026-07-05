using FluentValidation;

namespace SmartSpaces.Application.Features.Access.Commands.Scan;

public class ScanAccessCommandValidator : AbstractValidator<ScanAccessCommand>
{
    public ScanAccessCommandValidator()
    {
        RuleFor(x => x.QrToken)
            .NotEmpty().WithMessage("El qrToken es requerido.");

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("El deviceId es requerido.");

        RuleFor(x => x.Direction)
            .NotEmpty().WithMessage("La dirección es requerida.")
            .Must(d => d?.ToUpperInvariant() is "IN" or "OUT")
            .WithMessage("La dirección debe ser IN u OUT.");
    }
}
