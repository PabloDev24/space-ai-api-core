using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSpaces.Domain.Entities
{
    public class ClaseHorario
    {
        public Guid Id { get; set; }
        public Guid MateriaId { get; set; }
        public required string Grupo { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public required string Edificio { get; set; }
        public required string Salon { get; set; }
        public DayOfWeek DiaSemana { get; set; }

        public Materia? Materia { get; set; }
    }
}
