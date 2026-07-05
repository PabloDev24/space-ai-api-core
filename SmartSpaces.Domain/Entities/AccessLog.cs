using System;

namespace SmartSpaces.Domain.Entities
{
    public class AccessLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string DeviceId { get; set; }
        public required string Direction { get; set; } // IN | OUT
        public bool Granted { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
