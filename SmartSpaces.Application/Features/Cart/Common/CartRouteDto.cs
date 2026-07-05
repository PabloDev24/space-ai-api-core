using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Cart.Common;

public record CartRouteDto(
    string From,
    string To,
    int EstimatedTimeMinutes,
    IReadOnlyList<string> Steps,
    bool IsSimulated
)
{
    public static CartRouteDto FromRoute(CartRoute route) => new(
        route.From,
        route.To,
        route.EstimatedTimeMinutes,
        route.Steps,
        route.IsSimulated
    );
}
