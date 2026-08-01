using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.AiAssistant.Queries.GetPipeline;

public record GetAiPipelineQuery() : IRequest<IReadOnlyList<AiPipelineStageDto>>;

/// <summary>Status: Completed | InProgress | Pending | Error.</summary>
public record AiPipelineStageDto(
    string Id,
    string Name,
    string Description,
    string Status,
    string? Time,
    string? Documents
);

public class GetAiPipelineQueryHandler : IRequestHandler<GetAiPipelineQuery, IReadOnlyList<AiPipelineStageDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAiPipelineQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AiPipelineStageDto>> Handle(GetAiPipelineQuery request, CancellationToken cancellationToken)
    {
        // El pipeline real corre en el microservicio FastAPI; aquí se refleja el estado
        // agregado de los documentos, que es lo que este backend sí conoce.
        var counts = await _context.KnowledgeDocuments
            .AsNoTracking()
            .GroupBy(d => d.Status)
            .Select(g => new { Status = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Total, cancellationToken);

        var pending = counts.GetValueOrDefault(AiDocumentCatalog.Pending);
        var processing = counts.GetValueOrDefault(AiDocumentCatalog.Processing);
        var indexed = counts.GetValueOrDefault(AiDocumentCatalog.Indexed);
        var errored = counts.GetValueOrDefault(AiDocumentCatalog.Error);
        var total = pending + processing + indexed + errored;

        return
        [
            new AiPipelineStageDto(
                Id: "extraction",
                Name: "Extracción de texto",
                Description: pending > 0
                    ? "Documentos en cola esperando extracción."
                    : total > 0 ? "Texto y metadata extraídos de los documentos." : "Sin documentos en el repositorio.",
                Status: pending > 0 ? "InProgress" : total > 0 ? "Completed" : "Pending",
                Time: null,
                Documents: DocumentsLabel(pending > 0 ? pending : total)),

            new AiPipelineStageDto(
                Id: "chunking",
                Name: "Fragmentación semántica",
                Description: processing > 0
                    ? "Dividiendo documentos en fragmentos."
                    : indexed > 0 ? "Fragmentación completada." : "Esperando documentos extraídos.",
                Status: processing > 0 ? "InProgress" : indexed > 0 ? "Completed" : "Pending",
                Time: null,
                Documents: DocumentsLabel(processing)),

            new AiPipelineStageDto(
                Id: "embeddings",
                Name: "Generación de embeddings",
                Description: processing > 0
                    ? "Generando vectores de los fragmentos."
                    : indexed > 0 ? "Embeddings generados." : "Esperando fragmentos.",
                Status: processing > 0 ? "InProgress" : indexed > 0 ? "Completed" : "Pending",
                Time: null,
                Documents: DocumentsLabel(processing)),

            new AiPipelineStageDto(
                Id: "indexing",
                Name: "Indexación vectorial",
                Description: errored > 0
                    ? $"{errored} documento(s) fallaron al indexarse."
                    : indexed > 0 ? "Índice vectorial sincronizado." : "Esperando embeddings.",
                Status: errored > 0
                    ? "Error"
                    : indexed > 0 && pending == 0 && processing == 0 ? "Completed" : indexed > 0 ? "InProgress" : "Pending",
                Time: null,
                Documents: DocumentsLabel(indexed))
        ];
    }

    private static string? DocumentsLabel(int count) => count > 0 ? $"{count} doc" : null;
}
