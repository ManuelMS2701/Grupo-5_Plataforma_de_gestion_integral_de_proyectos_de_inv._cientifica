using System.ComponentModel.DataAnnotations;

namespace ResearchHub.Models
{
    public class Laboratorio
    {
        [Key]
        public int IdLaboratorio { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Ubicacion { get; set; }

        public int Capacidad { get; set; }

        [MaxLength(150)]
        public string? Responsable { get; set; }

        public bool Activo { get; set; }

        public ICollection<Experimento> Experimentos { get; set; } = new List<Experimento>();
        public ICollection<EquipoLaboratorio> Equipos { get; set; } = new List<EquipoLaboratorio>();
    }
}
