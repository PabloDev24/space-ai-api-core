using System;

namespace SmartSpaces.Domain.Entities
{
    public class KnowledgeQuery
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string Question { get; set; }
        public required string Answer { get; set; }
        public required string Source { get; set; }
        public double Confidence { get; set; }
        public bool IsMock { get; set; }
        public string? SourcesJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
