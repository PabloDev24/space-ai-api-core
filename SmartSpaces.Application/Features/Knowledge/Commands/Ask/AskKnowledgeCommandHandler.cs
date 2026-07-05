using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Domain.Entities;

namespace SmartSpaces.Application.Features.Knowledge.Commands.Ask;

public class AskKnowledgeCommandHandler : IRequestHandler<AskKnowledgeCommand, AskKnowledgeResult>
{
    private const string FallbackAnswer =
        "No pude consultar el motor de conocimiento en este momento. " +
        "Por favor intenta de nuevo en unos minutos o consulta con servicios escolares.";

    private readonly IApplicationDbContext _context;
    private readonly IRagService _ragService;

    public AskKnowledgeCommandHandler(IApplicationDbContext context, IRagService ragService)
    {
        _context = context;
        _ragService = ragService;
    }

    public async Task<AskKnowledgeResult> Handle(AskKnowledgeCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("El usuario especificado no existe.");
        }

        var userContext = new RagUserContext(user.Id.ToString(), user.Name, user.Role);

        AskKnowledgeResult result;
        try
        {
            var ragResult = await _ragService.AskAsync(request.Question, userContext, request.Source, cancellationToken);

            var sources = ragResult.Sources
                .Select(s => new KnowledgeSourceDto(s.Title, s.Page))
                .ToList();

            result = new AskKnowledgeResult(ragResult.Answer, ragResult.Confidence, sources, false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // El RAG no respondió a tiempo: fallback controlado en vez de romper la demo (docs/00 regla 8).
            result = new AskKnowledgeResult(FallbackAnswer, 0.5, Array.Empty<KnowledgeSourceDto>(), true);
        }

        await PersistQueryAsync(request, result, cancellationToken);

        return result;
    }

    private async Task PersistQueryAsync(AskKnowledgeCommand request, AskKnowledgeResult result, CancellationToken cancellationToken)
    {
        try
        {
            _context.KnowledgeQueries.Add(new KnowledgeQuery
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Question = request.Question,
                Answer = result.Answer,
                Source = request.Source,
                Confidence = result.Confidence,
                IsMock = result.IsMock,
                SourcesJson = result.Sources.Count > 0 ? JsonSerializer.Serialize(result.Sources) : null,
            });
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Guardar el historial no debe romper la respuesta al cliente (docs/00 regla 8).
        }
    }
}
