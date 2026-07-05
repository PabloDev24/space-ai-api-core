using SmartSpaces.Application.Common.Cart;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Infrastructure.Services;

// Un solo carrito físico en la demo: basta un estado en memoria (Singleton), sin base de datos.
public class InMemoryCartLastRouteTracker : ICartLastRouteTracker
{
    private readonly object _lock = new();
    private IReadOnlyList<ResolvedMovementStep>? _lastRoute;

    public void SetLastRoute(IReadOnlyList<ResolvedMovementStep> steps)
    {
        lock (_lock)
        {
            _lastRoute = steps;
        }
    }

    public IReadOnlyList<ResolvedMovementStep>? GetLastRoute()
    {
        lock (_lock)
        {
            return _lastRoute;
        }
    }
}
