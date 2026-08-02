using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Infrastructure.Services;

/// <summary>
/// Storage en disco local para los documentos del RAG. Suficiente mientras la API corre
/// nativa con `dotnet run`; al migrar a Azure se sustituye por una implementación de Blob
/// Storage sin tocar los handlers (ver KnowledgeDocuments:StoragePath en appsettings).
/// </summary>
public class LocalDocumentStorage : IDocumentStorage
{
    private const string DefaultRelativePath = "storage/knowledge-documents";

    private readonly string _rootPath;
    private readonly ILogger<LocalDocumentStorage> _logger;

    public LocalDocumentStorage(IConfiguration configuration, ILogger<LocalDocumentStorage> logger)
    {
        _logger = logger;

        var configured = configuration["KnowledgeDocuments:StoragePath"];
        var relativeOrAbsolute = string.IsNullOrWhiteSpace(configured) ? DefaultRelativePath : configured;

        _rootPath = Path.IsPathRooted(relativeOrAbsolute)
            ? relativeOrAbsolute
            : Path.Combine(Directory.GetCurrentDirectory(), relativeOrAbsolute);
    }

    public async Task<StoredDocument> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_rootPath);

        // El nombre original es dato del usuario: nunca se usa para construir la ruta (evita
        // que un "../../appsettings.json" escriba fuera del directorio). Se conserva en la DB.
        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_rootPath, storedFileName);

        await using (var target = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(target, cancellationToken);
        }

        var sizeBytes = new FileInfo(fullPath).Length;
        _logger.LogInformation("Documento almacenado en {Path} ({Bytes} bytes)", fullPath, sizeBytes);

        return new StoredDocument(fullPath, sizeBytes);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        try
        {
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }
        }
        catch (Exception ex)
        {
            // Un archivo huérfano no debe romper la operación que lo intentaba limpiar.
            _logger.LogWarning(ex, "No se pudo eliminar el archivo {Path}", storagePath);
        }

        return Task.CompletedTask;
    }
}
