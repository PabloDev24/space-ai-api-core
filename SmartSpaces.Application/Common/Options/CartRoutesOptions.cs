namespace SmartSpaces.Application.Common.Options;

// Catálogo de destinos del carrito para navegación guiada por voz.
// Cargado desde cart-routes.json (ver Program.cs) con reloadOnChange:true —
// agregar un destino nuevo es editar este JSON, sin tocar código (docs/00 §3.4).
public class CartRoutesOptions
{
    public CartCalibration Calibration { get; set; } = new();

    // Frases que disparan "regresar al punto de partida" (reverso de la última ruta enviada),
    // no atadas a un destino específico del catálogo.
    public List<string> ReturnAliases { get; set; } = [];

    public List<CartRouteDefinition> Destinations { get; set; } = [];
}

// Constantes físicas del carrito, medidas empíricamente probando en el hardware real.
// Recalibrar aquí (una sola vez) ajusta automáticamente todas las rutas del catálogo.
public class CartCalibration
{
    public double InchesPerSecond { get; set; } = 16.9;

    // Modelo lineal grados = TurnDegreesPerSecond * segundos + TurnOffsetDegrees
    // (el offset negativo refleja el tiempo muerto de arranque del giro).
    public double TurnDegreesPerSecond { get; set; } = 160;
    public double TurnOffsetDegrees { get; set; } = -38;
}

public class CartRouteDefinition
{
    public string DestinationCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Aliases { get; set; } = [];
    public List<CartMovementStep> Steps { get; set; } = [];
}

public class CartMovementStep
{
    public string Command { get; set; } = string.Empty; // adelante | atras | izquierda | derecha | stop

    // Define UNA de estas tres formas (en ese orden de prioridad si hay varias):
    public double? DurationSeconds { get; set; } // override manual directo
    public double? DistanceInches { get; set; }  // para adelante/atras
    public double? Degrees { get; set; }         // para izquierda/derecha
}
