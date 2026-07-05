using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartSpaces.Application.Common.Cart;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Common.Options;
using SmartSpaces.Application.Features.Knowledge.Commands.Ask;

namespace SmartSpaces.Application.Features.Cart.Commands.VoiceInteract;

// Conversación unificada por voz para el carrito: la misma entrada puede ser una orden de
// navegación ("guíame al edificio F", "llévame a la biblioteca") o una pregunta libre que ya
// resuelve el RAG. UserId es opcional: sesión ligera, sin login obligatorio (docs/00 §3.4).
public record VoiceCartCommand(Guid? UserId, string Transcript) : IRequest<VoiceCartResult>;

public record VoiceCartResult(
    string Kind, // "navigate" | "answer"
    string SpokenMessage, // se lee en voz alta (síntesis de voz) y se muestra en texto
    bool IsMock, // navegación simulada (broker inalcanzable) o fallback de RAG
    string? DestinationCode,
    string? DestinationDisplayName,
    double? EstimatedSeconds,
    IReadOnlyList<KnowledgeSourceDto>? Sources
);

public class VoiceCartCommandHandler : IRequestHandler<VoiceCartCommand, VoiceCartResult>
{
    private const string ReturnDestinationCode = "REGRESO_INICIO";
    private const string ReturnDisplayName = "el punto de partida";
    private const string GuestGreetingTemplate = "Te llevo a {0}.";
    private const string NamedGreetingTemplate = "Claro, {0}, te llevo a {1}.";
    private const string NoLastRouteMessage = "No tengo un recorrido previo para regresar.";
    private const string GuestFallbackAnswer =
        "No pude consultar el motor de conocimiento en este momento. Intenta de nuevo en unos minutos.";

    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly IRagService _ragService;
    private readonly ICartMovementPublisher _movementPublisher;
    private readonly ICartLastRouteTracker _lastRouteTracker;
    private readonly IOptionsMonitor<CartRoutesOptions> _routesOptions;

    public VoiceCartCommandHandler(
        IMediator mediator,
        IApplicationDbContext context,
        IRagService ragService,
        ICartMovementPublisher movementPublisher,
        ICartLastRouteTracker lastRouteTracker,
        IOptionsMonitor<CartRoutesOptions> routesOptions)
    {
        _mediator = mediator;
        _context = context;
        _ragService = ragService;
        _movementPublisher = movementPublisher;
        _lastRouteTracker = lastRouteTracker;
        _routesOptions = routesOptions;
    }

    public async Task<VoiceCartResult> Handle(VoiceCartCommand request, CancellationToken cancellationToken)
    {
        var transcript = request.Transcript?.Trim() ?? string.Empty;
        var options = _routesOptions.CurrentValue;

        if (MatchesAnyAlias(transcript, options.ReturnAliases))
        {
            return await HandleReturnAsync(request.UserId, cancellationToken);
        }

        var match = FindDestination(transcript, options.Destinations);
        if (match != null)
        {
            return await HandleNavigateAsync(request.UserId, match, options.Calibration, cancellationToken);
        }

        return await HandleAnswerAsync(request.UserId, transcript, cancellationToken);
    }

    private static bool MatchesAnyAlias(string transcript, IReadOnlyList<string> aliases)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return false;
        }

        return aliases.Any(alias =>
            !string.IsNullOrWhiteSpace(alias) && transcript.Contains(alias, StringComparison.OrdinalIgnoreCase));
    }

    // Sin scoring: gana el primer alias que aparezca como substring del transcript.
    // Colisiones de alias entre destinos son responsabilidad de quien edita cart-routes.json.
    public static CartRouteDefinition? FindDestination(string transcript, IReadOnlyList<CartRouteDefinition> destinations)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return null;
        }

        foreach (var destination in destinations)
        {
            foreach (var alias in destination.Aliases)
            {
                if (!string.IsNullOrWhiteSpace(alias) &&
                    transcript.Contains(alias, StringComparison.OrdinalIgnoreCase))
                {
                    return destination;
                }
            }
        }

        return null;
    }

    private async Task<VoiceCartResult> HandleNavigateAsync(
        Guid? userId, CartRouteDefinition destination, CartCalibration calibration, CancellationToken cancellationToken)
    {
        var resolvedSteps = CartMovementCalculator.Resolve(destination.Steps, calibration);
        var reached = await _movementPublisher.TryRunSequenceAsync(resolvedSteps, cancellationToken);
        _lastRouteTracker.SetLastRoute(resolvedSteps);

        var name = userId.HasValue ? await TryGetUserNameAsync(userId.Value, cancellationToken) : null;
        var message = string.IsNullOrWhiteSpace(name)
            ? string.Format(GuestGreetingTemplate, destination.DisplayName)
            : string.Format(NamedGreetingTemplate, name, destination.DisplayName);

        return new VoiceCartResult(
            Kind: "navigate",
            SpokenMessage: message,
            IsMock: !reached,
            DestinationCode: destination.DestinationCode,
            DestinationDisplayName: destination.DisplayName,
            EstimatedSeconds: resolvedSteps.Sum(s => s.DurationSeconds),
            Sources: null
        );
    }

    // "Regrésate": deshace la última ruta enviada (cualquiera que haya sido), no una fija.
    private async Task<VoiceCartResult> HandleReturnAsync(Guid? userId, CancellationToken cancellationToken)
    {
        var lastRoute = _lastRouteTracker.GetLastRoute();
        if (lastRoute == null || lastRoute.Count == 0)
        {
            return new VoiceCartResult(
                Kind: "answer",
                SpokenMessage: NoLastRouteMessage,
                IsMock: false,
                DestinationCode: null,
                DestinationDisplayName: null,
                EstimatedSeconds: null,
                Sources: null
            );
        }

        var reverseSteps = CartMovementCalculator.ComputeReverse(lastRoute);
        var reached = await _movementPublisher.TryRunSequenceAsync(reverseSteps, cancellationToken);
        // Queda registrado como "última ruta": un segundo "regrésate" la vuelve a deshacer.
        _lastRouteTracker.SetLastRoute(reverseSteps);

        var name = userId.HasValue ? await TryGetUserNameAsync(userId.Value, cancellationToken) : null;
        var message = string.IsNullOrWhiteSpace(name)
            ? string.Format(GuestGreetingTemplate, ReturnDisplayName)
            : string.Format(NamedGreetingTemplate, name, ReturnDisplayName);

        return new VoiceCartResult(
            Kind: "navigate",
            SpokenMessage: message,
            IsMock: !reached,
            DestinationCode: ReturnDestinationCode,
            DestinationDisplayName: ReturnDisplayName,
            EstimatedSeconds: reverseSteps.Sum(s => s.DurationSeconds),
            Sources: null
        );
    }

    private async Task<VoiceCartResult> HandleAnswerAsync(Guid? userId, string transcript, CancellationToken cancellationToken)
    {
        if (userId.HasValue)
        {
            // Usuario identificado: reutiliza el flujo RAG existente (answer + isMock + persistencia).
            var ask = new AskKnowledgeCommand(userId.Value, transcript, "CART");
            var answer = await _mediator.Send(ask, cancellationToken);

            return new VoiceCartResult(
                Kind: "answer",
                SpokenMessage: answer.Answer,
                IsMock: answer.IsMock,
                DestinationCode: null,
                DestinationDisplayName: null,
                EstimatedSeconds: null,
                Sources: answer.Sources
            );
        }

        // Invitado sin sesión: se consulta el RAG directo, sin persistir historial ligado
        // a un usuario que no existe (sesión ligera, sin cambio de esquema — docs/00).
        try
        {
            var guestContext = new RagUserContext("guest", "Invitado", "guest");
            var ragResult = await _ragService.AskAsync(transcript, guestContext, "CART", cancellationToken);

            return new VoiceCartResult(
                Kind: "answer",
                SpokenMessage: ragResult.Answer,
                IsMock: false,
                DestinationCode: null,
                DestinationDisplayName: null,
                EstimatedSeconds: null,
                Sources: ragResult.Sources.Select(s => new KnowledgeSourceDto(s.Title, s.Page)).ToList()
            );
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // El RAG no respondió a tiempo: fallback controlado en vez de romper la demo (docs/00 regla 8).
            return new VoiceCartResult(
                Kind: "answer",
                SpokenMessage: GuestFallbackAnswer,
                IsMock: true,
                DestinationCode: null,
                DestinationDisplayName: null,
                EstimatedSeconds: null,
                Sources: Array.Empty<KnowledgeSourceDto>()
            );
        }
    }

    private async Task<string?> TryGetUserNameAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return user?.Name;
    }
}
