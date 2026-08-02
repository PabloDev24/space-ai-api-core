using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSpaces.Domain.Entities
{
    public class Materia
    {
        public Guid Id { get; set; }
        public required string Nombre { get; set; }
        public required string Profesor { get; set; }

        public ICollection<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
        public ICollection<ClaseHorario> Clases { get; set; } = new List<ClaseHorario>();
    }
}
