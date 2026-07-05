using SmartSpaces.Application.Common.Cart;

namespace SmartSpaces.Application.Common.Interfaces;

// Recuerda la última secuencia de movimiento enviada al carrito físico (uno solo, en memoria,
// sin persistencia) para poder calcular su reverso exacto ante un "regrésate" genérico.
public interface ICartLastRouteTracker
{
    void SetLastRoute(IReadOnlyList<ResolvedMovementStep> steps);
    IReadOnlyList<ResolvedMovementStep>? GetLastRoute();
}
