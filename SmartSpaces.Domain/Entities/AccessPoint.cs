using System;

namespace SmartSpaces.Domain.Entities
{
    /// <summary>
    /// Punto de acceso físico (pluma, torniquete, puerta). Se distingue de <see cref="Device"/>
    /// en que representa la instalación en sitio, no el hardware genérico registrado en la red.
    /// </summary>
    public class AccessPoint
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Building { get; set; }

        /// <summary>
        /// Identificador que envía el lector al escanear. Empata contra <see cref="AccessLog.DeviceId"/>
        /// para derivar escaneos del día y última validación sin duplicar contadores en esta tabla.
        /// </summary>
        public required string DeviceId { get; set; }

        public required string Status { get; set; } // Active | Inactive | Maintenance | Reader Fault
        public int NetworkPingMs { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
