using MediatR;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Cart.Queries.GetStatus;

public record GetCartStatusQuery() : IRequest<CartStatusDto>;

public record CartStatusDto(
    string Id,
    string Status,
    string Mode,
    int Battery,
    string CurrentZone,
    bool IsSimulated
);

public class GetCartStatusQueryHandler : IRequestHandler<GetCartStatusQuery, CartStatusDto>
{
    private readonly ICartService _cartService;

    public GetCartStatusQueryHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public Task<CartStatusDto> Handle(GetCartStatusQuery request, CancellationToken cancellationToken)
    {
        var status = _cartService.GetStatus();
        return Task.FromResult(new CartStatusDto(
            status.Id,
            status.Status,
            status.Mode,
            status.Battery,
            status.CurrentZone,
            status.IsSimulated
        ));
    }
}
