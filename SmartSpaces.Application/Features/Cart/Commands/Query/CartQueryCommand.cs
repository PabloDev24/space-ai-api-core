using MediatR;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.Cart.Common;
using SmartSpaces.Application.Features.Knowledge.Commands.Ask;

namespace SmartSpaces.Application.Features.Cart.Commands.Query;

public record CartQueryCommand(Guid UserId, string Question, string? TargetZone) : IRequest<CartQueryResult>;

public record CartQueryResult(string Answer, CartRouteDto? Route, bool IsMock);

public class CartQueryCommandHandler : IRequestHandler<CartQueryCommand, CartQueryResult>
{
    private readonly IMediator _mediator;
    private readonly ICartService _cartService;

    public CartQueryCommandHandler(IMediator mediator, ICartService cartService)
    {
        _mediator = mediator;
        _cartService = cartService;
    }

    public async Task<CartQueryResult> Handle(CartQueryCommand request, CancellationToken cancellationToken)
    {
        // Reutiliza el flujo RAG del módulo Knowledge: answer + isMock + persistencia
        // (source CART) + fallback controlado, sin duplicar lógica.
        var ask = new AskKnowledgeCommand(request.UserId, request.Question, "CART");
        var answer = await _mediator.Send(ask, cancellationToken);

        CartRouteDto? route = null;
        if (!string.IsNullOrWhiteSpace(request.TargetZone))
        {
            var status = _cartService.GetStatus();
            route = CartRouteDto.FromRoute(_cartService.BuildRoute(status.CurrentZone, request.TargetZone));
        }

        return new CartQueryResult(answer.Answer, route, answer.IsMock);
    }
}
