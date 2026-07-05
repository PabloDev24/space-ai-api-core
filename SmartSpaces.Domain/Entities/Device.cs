using System;

namespace SmartSpaces.Domain.Entities
{
    public class Device
    {
        public Guid Id { get; set; }
        public required string Code { get; set; } // e.g. "cart-tablet-001"
        public required string Name { get; set; }
        public required string Type { get; set; } // SIDE | CART | ACCESS | SENSOR | CAMERA | GATEWAY | KIOSK
        public required string Status { get; set; } // ONLINE | OFFLINE
        public string? Location { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
