using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Formatting;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Domain.Entities;

namespace SmartSpaces.Application.Features.AiAssistant.Queries.GetDocuments;

public record GetAiDocumentsQuery() : IRequest<IReadOnlyList<AiDocumentDto>>;

/// <summary>
/// AddedAt / Size / LastSynced van como texto ya formateado porque el panel los pinta en crudo;
/// Timestamp (epoch ms) es el que usa para ordenar.
/// </summary>
public record AiDocumentDto(
    string Id,
    string Name,
    string Type,
    string Status,
    string Size,
    string AddedAt,
    long Timestamp,
    string LastSynced
);

public class GetAiDocumentsQueryHandler : IRequestHandler<GetAiDocumentsQuery, IReadOnlyList<AiDocumentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAiDocumentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AiDocumentDto>> Handle(GetAiDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _context.KnowledgeDocuments
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return documents.Select(ToDto).ToList();
    }

    public static AiDocumentDto ToDto(KnowledgeDocument document) => new(
        Id: document.Id.ToString(),
        Name: document.Name,
        Type: document.Extension,
        Status: document.Status,
        Size: DisplayFormat.Bytes(document.SizeBytes),
        AddedAt: DisplayFormat.RelativeTime(document.CreatedAt),
        Timestamp: new DateTimeOffset(DateTime.SpecifyKind(document.CreatedAt, DateTimeKind.Utc)).ToUnixTimeMilliseconds(),
        LastSynced: FormatLastSynced(document));

    private static string FormatLastSynced(KnowledgeDocument document) => document.Status switch
    {
        AiDocumentCatalog.Indexed => DisplayFormat.RelativeTime(document.LastSyncedAt, emptyLabel: "Sin sincronizar"),
        AiDocumentCatalog.Processing => "Procesando...",
        AiDocumentCatalog.Error => "Error indexación",
        _ => "Pendiente"
    };
}
