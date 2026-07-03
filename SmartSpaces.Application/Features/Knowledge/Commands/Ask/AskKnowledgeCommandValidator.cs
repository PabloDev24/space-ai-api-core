using FluentValidation;

namespace SmartSpaces.Application.Features.Knowledge.Commands.Ask;

public class AskKnowledgeCommandValidator : AbstractValidator<AskKnowledgeCommand>
{
    public AskKnowledgeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El userId es requerido.");

        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("La pregunta no puede estar vacía.")
            .MaximumLength(1000).WithMessage("La pregunta no puede tener más de 1000 caracteres.");

        RuleFor(x => x.Source)
            .NotEmpty().WithMessage("El source es requerido.");
    }
}
