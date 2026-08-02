using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSpaces.Domain.Entities
{
    public class Calificacion
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid MateriaId { get; set; }
        public double? Parcial1 { get; set; }
        public double? Parcial2 { get; set; }
        public double? Parcial3 { get; set; }
        public double? Final { get; set; }

        public User? User { get; set; }
        public Materia? Materia { get; set; }
    }
}
