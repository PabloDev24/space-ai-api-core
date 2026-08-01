namespace SmartSpaces.Application.Common.Formatting;

/// <summary>
/// Formatos que el panel Angular pinta tal cual (sin pipe de fecha ni de número):
/// AccessPoint.lastValidation, AIDocument.addedAt / size / lastSynced, AIActivityEvent.timestamp.
/// Se generan aquí para que la vista no tenga que reimplementar la misma lógica en TS.
/// </summary>
public static class DisplayFormat
{
    /// <summary>"Justo ahora" | "Hace 5 min" | "Hace 3 h" | "Ayer" | "Hace 4 días" | "12/06/2026".</summary>
    public static string RelativeTime(DateTime? utcTimestamp, string emptyLabel = "Sin registros")
    {
        if (utcTimestamp is null)
        {
            return emptyLabel;
        }

        var elapsed = DateTime.UtcNow - utcTimestamp.Value;

        if (elapsed < TimeSpan.Zero)
        {
            return "Justo ahora";
        }

        if (elapsed < TimeSpan.FromMinutes(1))
        {
            return "Justo ahora";
        }

        if (elapsed < TimeSpan.FromHours(1))
        {
            var minutes = (int)elapsed.TotalMinutes;
            return $"Hace {minutes} min";
        }

        if (elapsed < TimeSpan.FromDays(1))
        {
            var hours = (int)elapsed.TotalHours;
            return hours == 1 ? "Hace 1 hora" : $"Hace {hours} horas";
        }

        var days = (int)elapsed.TotalDays;

        if (days == 1)
        {
            return "Ayer";
        }

        if (days < 7)
        {
            return $"Hace {days} días";
        }

        if (days < 30)
        {
            var weeks = days / 7;
            return weeks == 1 ? "Hace 1 sem" : $"Hace {weeks} sem";
        }

        return utcTimestamp.Value.ToString("dd/MM/yyyy");
    }

    /// <summary>"45 KB" | "2.4 MB" | "1.3 GB".</summary>
    public static string Bytes(long bytes)
    {
        if (bytes <= 0)
        {
            return "0 KB";
        }

        const long kilobyte = 1024;
        const long megabyte = kilobyte * 1024;
        const long gigabyte = megabyte * 1024;

        if (bytes >= gigabyte)
        {
            return $"{bytes / (double)gigabyte:0.#} GB";
        }

        if (bytes >= megabyte)
        {
            return $"{bytes / (double)megabyte:0.#} MB";
        }

        return $"{Math.Max(1, bytes / kilobyte)} KB";
    }

    /// <summary>
    /// Variación porcentual de un periodo contra el anterior, con un decimal.
    /// Devuelve 0 cuando no hay base de comparación (evita divisiones por cero y "+∞%" en la UI).
    /// </summary>
    public static double GrowthPercentage(int current, int previous)
    {
        if (previous <= 0)
        {
            return 0;
        }

        return Math.Round((current - previous) / (double)previous * 100, 1);
    }
}
