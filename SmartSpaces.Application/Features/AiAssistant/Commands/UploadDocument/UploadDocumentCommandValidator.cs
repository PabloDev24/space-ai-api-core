using FluentValidation;

namespace SmartSpaces.Application.Features.AiAssistant.Commands.UploadDocument;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("El nombre del archivo es obligatorio.")
            .MaximumLength(260).WithMessage("El nombre del archivo no puede exceder 260 caracteres.")
            .Must(AiDocumentCatalog.IsAllowedFile)
            .WithMessage($"Formato no soportado. Se admiten: {string.Join(", ", AiDocumentCatalog.AllowedExtensions)}.");

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("El archivo está vacío.")
            .LessThanOrEqualTo(AiDocumentCatalog.MaxFileSizeBytes)
            .WithMessage($"El archivo excede el máximo de {AiDocumentCatalog.MaxFileSizeBytes / (1024 * 1024)} MB.");

        RuleFor(x => x.Content)
            .NotNull().WithMessage("No se recibió el contenido del archivo.");
    }
}
