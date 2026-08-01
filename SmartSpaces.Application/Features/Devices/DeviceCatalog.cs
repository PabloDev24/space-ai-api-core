namespace SmartSpaces.Application.Features.Devices;

/// <summary>
/// Valores admitidos para Device.Type / Device.Status (ver comentarios de la entidad Device).
/// Centralizados aquí porque los validan tanto el alta como la edición.
/// </summary>
public static class DeviceCatalog
{
    public static readonly string[] Types = ["SIDE", "CART", "ACCESS", "SENSOR", "CAMERA", "GATEWAY", "KIOSK"];

    public static readonly string[] Statuses = ["ONLINE", "OFFLINE"];

    public static bool IsValidType(string? type) =>
        !string.IsNullOrWhiteSpace(type) && Types.Contains(type.ToUpperInvariant());

    public static bool IsValidStatus(string? status) =>
        !string.IsNullOrWhiteSpace(status) && Statuses.Contains(status.ToUpperInvariant());
}
