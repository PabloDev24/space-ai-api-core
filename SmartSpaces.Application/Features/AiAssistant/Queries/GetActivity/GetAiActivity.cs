using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Formatting;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.AiAssistant.Queries.GetActivity;

public record GetAiActivityQuery(int Limit = 20) : IRequest<IReadOnlyList<AiActivityEventDto>>;

/// <summary>
/// Type:   Upload | Index | Error (Delete y Sync no se emiten: no hay endpoint de borrado
///         ni callback de sincronización del RAG todavía).
/// Status: Success | Error | Warning | Info.
/// </summary>
public record AiActivityEventDto(
    string Id,
    string Type,
    string Description,
    string Timestamp,
    string Status
);

public class GetAiActivityQueryHandler : IRequestHandler<GetAiActivityQuery, IReadOnlyList<AiActivityEventDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAiActivityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AiActivityEventDto>> Handle(GetAiActivityQuery request, CancellationToken cancellationToken)
    {
        var limit = request.Limit is > 0 and <= 100 ? request.Limit : 20;

        var documents = await _context.KnowledgeDocuments
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .Take(limit)
            .Select(d => new { d.Id, d.Name, d.Status, d.ErrorMessage, d.CreatedAt, d.LastSyncedAt })
            .ToListAsync(cancellationToken);

        var events = new List<(DateTime At, AiActivityEventDto Event)>();

        foreach (var document in documents)
        {
            events.Add((document.CreatedAt, new AiActivityEventDto(
                Id: $"upload-{document.Id}",
                Type: "Upload",
                Description: $"Documento {document.Name} agregado al repositorio",
                Timestamp: DisplayFormat.RelativeTime(document.CreatedAt),
                Status: "Success")));

            if (document.Status == AiDocumentCatalog.Indexed && document.LastSyncedAt.HasValue)
            {
                events.Add((document.LastSyncedAt.Value, new AiActivityEventDto(
                    Id: $"index-{document.Id}",
                    Type: "Index",
                    Description: $"{document.Name} vectorizado e indexado",
                    Timestamp: DisplayFormat.RelativeTime(document.LastSyncedAt),
                    Status: "Success")));
            }

            if (document.Status == AiDocumentCatalog.Error)
            {
                var failedAt = document.LastSyncedAt ?? document.CreatedAt;

                events.Add((failedAt, new AiActivityEventDto(
                    Id: $"error-{document.Id}",
                    Type: "Error",
                    Description: string.IsNullOrWhiteSpace(document.ErrorMessage)
                        ? $"Falla al procesar {document.Name}"
                        : document.ErrorMessage,
                    Timestamp: DisplayFormat.RelativeTime(failedAt),
                    Status: "Error")));
            }
        }

        return events
            .OrderByDescending(e => e.At)
            .Take(limit)
            .Select(e => e.Event)
            .ToList();
    }
}
