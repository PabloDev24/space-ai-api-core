using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Formatting;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.AiAssistant.Queries.GetMetrics;

public record GetAiMetricsQuery() : IRequest<AiMetricsDto>;

public record AiMetricsDto(
    int TotalDocuments,
    string RepositorySize,
    string IndexStatus,
    int QueriesAnswered,
    int ConfidenceRate,
    int IdentifiedGaps
);

public class GetAiMetricsQueryHandler : IRequestHandler<GetAiMetricsQuery, AiMetricsDto>
{
    /// <summary>
    /// Debajo de esta confianza la respuesta se considera un hueco de conocimiento:
    /// el RAG contestó, pero sin respaldo suficiente en los documentos base.
    /// </summary>
    private const double LowConfidenceThreshold = 0.5;

    private readonly IApplicationDbContext _context;

    public GetAiMetricsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AiMetricsDto> Handle(GetAiMetricsQuery request, CancellationToken cancellationToken)
    {
        var totalDocuments = await _context.KnowledgeDocuments.CountAsync(cancellationToken);

        var repositoryBytes = totalDocuments == 0
            ? 0
            : await _context.KnowledgeDocuments.SumAsync(d => d.SizeBytes, cancellationToken);

        var indexedDocuments = await _context.KnowledgeDocuments
            .CountAsync(d => d.Status == AiDocumentCatalog.Indexed, cancellationToken);

        var queriesAnswered = await _context.KnowledgeQueries.CountAsync(cancellationToken);

        // Cast a double? para que un repositorio sin consultas devuelva null en vez de reventar.
        var averageConfidence = await _context.KnowledgeQueries
            .Select(q => (double?)q.Confidence)
            .AverageAsync(cancellationToken);

        var identifiedGaps = await _context.KnowledgeQueries
            .CountAsync(q => q.Confidence < LowConfidenceThreshold, cancellationToken);

        var indexStatus = totalDocuments == 0
            ? "0%"
            : $"{indexedDocuments * 100.0 / totalDocuments:0.#}%";

        return new AiMetricsDto(
            TotalDocuments: totalDocuments,
            RepositorySize: DisplayFormat.Bytes(repositoryBytes),
            IndexStatus: indexStatus,
            QueriesAnswered: queriesAnswered,
            // El RAG devuelve confianza en escala 0–1; el panel la pinta como porcentaje entero.
            ConfidenceRate: (int)Math.Round((averageConfidence ?? 0) * 100),
            IdentifiedGaps: identifiedGaps);
    }
}
