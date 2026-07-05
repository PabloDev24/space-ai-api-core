using SmartSpaces.Application.Common.Cart;

namespace SmartSpaces.Application.Common.Interfaces;

// Publica una secuencia temporizada de comandos de movimiento al carrito (MQTT/ESP32).
// No debe lanzar: si el broker no responde, devuelve false y el llamador cae a modo simulado
// (mismo patrón de fallback controlado que ICartService/AskKnowledgeCommand, docs/00 regla 8).
public interface ICartMovementPublisher
{
    Task<bool> TryRunSequenceAsync(IReadOnlyList<ResolvedMovementStep> steps, CancellationToken cancellationToken);
}
