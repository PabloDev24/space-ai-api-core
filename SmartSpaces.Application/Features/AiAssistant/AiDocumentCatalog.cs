namespace SmartSpaces.Application.Features.AiAssistant;

/// <summary>
/// Estados del ciclo de vida de un documento base, con el mismo texto que espera el panel
/// (AIDocumentStatus en ai-assistant.interface.ts).
/// </summary>
public static class AiDocumentCatalog
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Indexed = "Indexed";
    public const string Error = "Error";

    /// <summary>Formatos que el pipeline del RAG sabe procesar.</summary>
    public static readonly string[] AllowedExtensions = ["pdf", "docx", "doc", "txt", "md"];

    /// <summary>Tope por archivo (25 MB), alineado con el límite de request de Kestrel.</summary>
    public const long MaxFileSizeBytes = 25 * 1024 * 1024;

    public static bool IsAllowedFile(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    /// <summary>"informe.PDF" → "PDF" (lo que el panel pinta en la columna Tipo).</summary>
    public static string ExtensionLabel(string fileName)
    {
        var extension = Path.GetExtension(fileName).TrimStart('.');
        return string.IsNullOrWhiteSpace(extension) ? "—" : extension.ToUpperInvariant();
    }
}
