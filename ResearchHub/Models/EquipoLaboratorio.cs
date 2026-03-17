using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class EquipoLaboratorio
    {
        [Key]
        public int IdEquipo { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Marca { get; set; }

        [MaxLength(100)]
        public string? Modelo { get; set; }

        [MaxLength(100)]
        public string? NumeroSerie { get; set; }

        public DateTime? FechaAdquisicion { get; set; }

        [MaxLength(50)]
        public string? Estado { get; set; }

        [ForeignKey(nameof(Laboratorio))]
        public int IdLaboratorio { get; set; }

        public Laboratorio? Laboratorio { get; set; }
    }
}
