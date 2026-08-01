using System;

namespace SmartSpaces.Domain.Entities
{
    /// <summary>
    /// Documento base del RAG. Esta tabla es el registro/metadata del archivo; el binario vive
    /// en el storage (ver IDocumentStorage) y la vectorización la hace el microservicio FastAPI.
    /// </summary>
    public class KnowledgeDocument
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Extension { get; set; }   // PDF | DOCX | TXT | MD
        public required string ContentType { get; set; }
        public long SizeBytes { get; set; }
        public required string StoragePath { get; set; }
        public required string Status { get; set; }      // Pending | Processing | Indexed | Error
        public string? ErrorMessage { get; set; }
        public Guid? UploadedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Última vez que el RAG confirmó la sincronización de este documento.</summary>
        public DateTime? LastSyncedAt { get; set; }
    }
}
