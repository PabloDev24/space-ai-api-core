namespace SmartSpaces.Application.Common.Interfaces;

public record StoredDocument(string StoragePath, long SizeBytes);

/// <summary>
/// Guarda el binario de los documentos base del RAG. La metadata vive en la tabla
/// KnowledgeDocuments; esta abstracción existe para poder cambiar disco local por
/// Azure Blob Storage sin tocar la capa Application.
/// </summary>
public interface IDocumentStorage
{
    Task<StoredDocument> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken);
}
