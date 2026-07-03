using MediatR;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.Cart.Common;

namespace SmartSpaces.Application.Features.Cart.Commands.Navigate;

public record NavigateCartCommand(Guid UserId, string TargetZone, string? Mode) : IRequest<NavigateCartResult>;

public record NavigateCartResult(bool Accepted, CartRouteDto Route);

public class NavigateCartCommandHandler : IRequestHandler<NavigateCartCommand, NavigateCartResult>
{
    private readonly ICartService _cartService;

    public NavigateCartCommandHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public Task<NavigateCartResult> Handle(NavigateCartCommand request, CancellationToken cancellationToken)
    {
        var status = _cartService.GetStatus();
        var route = _cartService.BuildRoute(status.CurrentZone, request.TargetZone);
        return Task.FromResult(new NavigateCartResult(true, CartRouteDto.FromRoute(route)));
    }
}
