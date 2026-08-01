using FluentValidation;

namespace SmartSpaces.Application.Features.Users.Commands.UpdateUserStatus;

public class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
{
    public UpdateUserStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador del usuario es obligatorio.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("El estado es obligatorio.")
            .Must(UserStatusCatalog.IsValid)
            .WithMessage($"El estado debe ser uno de: {string.Join(", ", UserStatusCatalog.All)}.");
    }
}
