using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

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

        try
        {
            var result = await _ragService.AskAsync(request.Question, userContext, request.Source, cancellationToken);

            var sources = result.Sources
                .Select(s => new KnowledgeSourceDto(s.Title, s.Page))
                .ToList();

            return new AskKnowledgeResult(result.Answer, result.Confidence, sources, false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // El RAG no respondió a tiempo: fallback controlado en vez de romper la demo (docs/00 regla 8).
            return new AskKnowledgeResult(FallbackAnswer, 0.5, Array.Empty<KnowledgeSourceDto>(), true);
        }
    }
}
