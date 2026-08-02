namespace SmartSpaces.Application.Features.Access;

/// <summary>
/// Estados admitidos para un punto de acceso. Se guardan con el mismo texto que espera
/// el panel (AccessStatus en access-control.interface.ts).
/// </summary>
public static class AccessPointCatalog
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string Maintenance = "Maintenance";
    public const string ReaderFault = "Reader Fault";

    public static readonly string[] Statuses = [Active, Inactive, Maintenance, ReaderFault];

    public static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return Statuses.FirstOrDefault(s => string.Equals(s, status.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsValidStatus(string? status) => NormalizeStatus(status) != null;
}
