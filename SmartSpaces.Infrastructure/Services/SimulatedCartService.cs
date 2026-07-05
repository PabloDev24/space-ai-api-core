using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Infrastructure.Services;

// Implementación SIMULADA del carrito. No hay hardware ni navegación real
// (ROS2/SLAM/RPLIDAR son fase futura, docs/00 §3.4). Todo lo que devuelve
// está marcado con IsSimulated=true para no presentarlo como real.
public class SimulatedCartService : ICartService
{
    private const string CartId = "cart-tablet-001";
    private const string HomeZone = "Entrada Principal";

    public CartStatus GetStatus()
    {
        return new CartStatus(
            Id: CartId,
            Status: "READY",
            Mode: "MANUAL_ASSISTED",
            Battery: 85,
            CurrentZone: HomeZone,
            IsSimulated: true
        );
    }

    public CartRoute BuildRoute(string from, string to)
    {
        var origin = string.IsNullOrWhiteSpace(from) ? HomeZone : from;
        var destination = string.IsNullOrWhiteSpace(to) ? HomeZone : to;

        // Pasos genéricos simulados: no es pathfinding real, solo guía asistida.
        var steps = new List<string>
        {
            $"Salir de {origin} por el pasillo principal.",
            "Continuar de frente hasta el punto de referencia central.",
            $"Girar hacia el área correspondiente y avanzar hasta {destination}.",
            $"Has llegado a {destination}."
        };

        return new CartRoute(
            From: origin,
            To: destination,
            EstimatedTimeMinutes: 4,
            Steps: steps,
            IsSimulated: true
        );
    }
}
