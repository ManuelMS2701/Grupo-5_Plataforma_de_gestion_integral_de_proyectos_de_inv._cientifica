using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class Cronograma
    {
        [Key]
        public int IdCronograma { get; set; }

        [Required, MaxLength(150)]
        public string NombreFase { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [MaxLength(50)]
        public string? Estado { get; set; }

        [ForeignKey(nameof(Proyecto))]
        public int IdProyecto { get; set; }

        public Proyecto? Proyecto { get; set; }
    }
}
