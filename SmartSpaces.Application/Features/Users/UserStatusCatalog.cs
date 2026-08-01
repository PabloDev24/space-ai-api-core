namespace SmartSpaces.Application.Features.Users;

/// <summary>
/// Valores admitidos para User.Status. Se guardan tal cual los manda el panel
/// ("Activo" | "Inactivo") para no tener que traducir en la vista.
/// </summary>
public static class UserStatusCatalog
{
    public const string Active = "Activo";
    public const string Inactive = "Inactivo";

    public static readonly string[] All = [Active, Inactive];

    /// <summary>Normaliza sin importar mayúsculas/acentos de entrada; null si no es un estado válido.</summary>
    public static string? Normalize(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return All.FirstOrDefault(s => string.Equals(s, status.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsValid(string? status) => Normalize(status) != null;
}
