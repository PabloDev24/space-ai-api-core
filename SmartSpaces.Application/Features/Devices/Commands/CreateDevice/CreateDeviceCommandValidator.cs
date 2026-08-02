using FluentValidation;

namespace SmartSpaces.Application.Features.Devices.Commands.CreateDevice;

public class CreateDeviceCommandValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código del dispositivo es obligatorio.")
            .MaximumLength(100).WithMessage("El código no puede exceder 100 caracteres.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del dispositivo es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("El tipo de dispositivo es obligatorio.")
            .Must(DeviceCatalog.IsValidType)
            .WithMessage($"El tipo debe ser uno de: {string.Join(", ", DeviceCatalog.Types)}.");

        RuleFor(x => x.Status)
            .Must(DeviceCatalog.IsValidStatus)
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage($"El estado debe ser uno de: {string.Join(", ", DeviceCatalog.Statuses)}.");

        RuleFor(x => x.Location)
            .MaximumLength(150).WithMessage("La ubicación no puede exceder 150 caracteres.");
    }
}
