using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class Muestra
    {
        [Key]
        public int IdMuestra { get; set; }

        [Required, MaxLength(100)]
        public string Codigo { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Tipo { get; set; }

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        public DateTime? FechaRecoleccion { get; set; }

        [MaxLength(200)]
        public string? Origen { get; set; }

        [MaxLength(200)]
        public string? Condicion { get; set; }

        [ForeignKey(nameof(Proyecto))]
        public int IdProyecto { get; set; }

        public Proyecto? Proyecto { get; set; }
    }
}
