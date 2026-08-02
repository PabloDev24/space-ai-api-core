using FluentValidation;

namespace SmartSpaces.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Telefono)
            .Matches(@"^\d{10}$").When(x => !string.IsNullOrEmpty(x.Telefono))
            .WithMessage("El teléfono debe tener 10 dígitos.");

        RuleFor(x => x.EmailAlterno)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.EmailAlterno))
            .WithMessage("El email alterno no es válido.");
    }
}