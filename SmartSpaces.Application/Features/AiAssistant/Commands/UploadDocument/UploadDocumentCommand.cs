using MediatR;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.AiAssistant.Queries.GetDocuments;
using SmartSpaces.Domain.Entities;

namespace SmartSpaces.Application.Features.AiAssistant.Commands.UploadDocument;

/// <summary>
/// Subida de un documento a la base de conocimiento. El archivo queda almacenado y registrado
/// en estado Pending: la vectorización la ejecuta el microservicio FastAPI del RAG, que es
/// quien luego marca el documento como Indexed.
/// </summary>
public record UploadDocumentCommand(
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content,
    Guid? UploadedByUserId = null
) : IRequest<AiDocumentDto>;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, AiDocumentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentStorage _storage;

    public UploadDocumentCommandHandler(IApplicationDbContext context, IDocumentStorage storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<AiDocumentDto> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(request.FileName).Trim();

        var stored = await _storage.SaveAsync(request.Content, fileName, cancellationToken);

        var document = new KnowledgeDocument
        {
            Id = Guid.NewGuid(),
            Name = fileName,
            Extension = AiDocumentCatalog.ExtensionLabel(fileName),
            ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType,
            // El tamaño real lo reporta el storage, no el header del cliente.
            SizeBytes = stored.SizeBytes,
            StoragePath = stored.StoragePath,
            Status = AiDocumentCatalog.Pending,
            UploadedByUserId = request.UploadedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.KnowledgeDocuments.Add(document);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Si el registro no se pudo guardar, el archivo en disco quedaría huérfano
            // y contaría como espacio ocupado sin aparecer en ninguna vista.
            await _storage.DeleteAsync(stored.StoragePath, CancellationToken.None);
            throw;
        }

        return GetAiDocumentsQueryHandler.ToDto(document);
    }
}
