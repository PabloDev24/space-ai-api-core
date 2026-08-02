using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSpaces.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public string? Folio {  get; set; }
        public required string Role { get; set; }

        /// <summary>
        /// Activo | Inactivo. Un usuario Inactivo conserva su registro pero no puede iniciar sesión
        /// (ver LoginQueryHandler) — es el switch "Activar/Desactivar" del panel de usuarios.
        /// </summary>
        public string Status { get; set; } = "Activo";

        public string? QrToken { get; set; }
        public DateTime QrExpiry {  get; set; }

        //Nuevo

        public string? Matricula { get; set; }
        public string? Carrera { get; set; }
        public string? Grupo { get; set; }
        public string? Division { get; set; }
        public string? Campus { get; set; }
        public string? Telefono { get; set; }
        public string? EmailAlterno { get; set; }
        public int TotalAttendance { get; set; }

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
