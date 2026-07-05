using SmartSpaces.Application.Common.Options;

namespace SmartSpaces.Application.Common.Cart;

// Paso ya resuelto a segundos reales, listo para publicarse por MQTT.
public record ResolvedMovementStep(string Command, double DurationSeconds);

// Convierte los pasos del catálogo (definidos en pulgadas/grados) a segundos usando la
// calibración del carrito, y calcula el reverso exacto de una secuencia ya ejecutada
// (para el comando genérico "regrésate" — deshace cualquier ruta, no solo una fija).
public static class CartMovementCalculator
{
    private const string Stop = "stop";

    public static List<ResolvedMovementStep> Resolve(IEnumerable<CartMovementStep> steps, CartCalibration calibration) =>
        steps.Select(step => new ResolvedMovementStep(step.Command, ResolveDuration(step, calibration))).ToList();

    public static double ResolveDuration(CartMovementStep step, CartCalibration calibration)
    {
        if (step.DurationSeconds.HasValue)
        {
            return Math.Max(step.DurationSeconds.Value, 0);
        }

        if (step.DistanceInches.HasValue && calibration.InchesPerSecond > 0)
        {
            return Math.Max(step.DistanceInches.Value / calibration.InchesPerSecond, 0);
        }

        if (step.Degrees.HasValue && calibration.TurnDegreesPerSecond > 0)
        {
            var seconds = (step.Degrees.Value - calibration.TurnOffsetDegrees) / calibration.TurnDegreesPerSecond;
            return Math.Max(seconds, 0);
        }

        return 0; // "stop" u otro comando sin parámetro
    }

    // Invierte orden y dirección de una secuencia ya resuelta: adelante<->atras,
    // izquierda<->derecha, misma duración (deshace exactamente el desplazamiento/giro).
    public static List<ResolvedMovementStep> ComputeReverse(IReadOnlyList<ResolvedMovementStep> steps)
    {
        var reversed = steps
            .Where(s => !IsStop(s.Command))
            .Reverse()
            .Select(s => new ResolvedMovementStep(InvertCommand(s.Command), s.DurationSeconds))
            .ToList();

        reversed.Add(new ResolvedMovementStep(Stop, 0));
        return reversed;
    }

    private static bool IsStop(string command) => string.Equals(command, Stop, StringComparison.OrdinalIgnoreCase);

    private static string InvertCommand(string command) => command.ToLowerInvariant() switch
    {
        "adelante" => "atras",
        "atras" => "adelante",
        "izquierda" => "derecha",
        "derecha" => "izquierda",
        _ => command,
    };
}
