using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class Publicacion
    {
        [Key]
        public int IdPublicacion { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Resumen { get; set; }

        [MaxLength(200)]
        public string? Revista { get; set; }

        public DateTime? FechaPublicacion { get; set; }

        [MaxLength(100)]
        public string? DOI { get; set; }

        [ForeignKey(nameof(Proyecto))]
        public int IdProyecto { get; set; }

        public Proyecto? Proyecto { get; set; }
    }
}
