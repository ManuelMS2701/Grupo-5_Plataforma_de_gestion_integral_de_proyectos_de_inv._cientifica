using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class RepositorioDatos
    {
        [Key]
        public int IdRepositorio { get; set; }

        [Required, MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        [MaxLength(500)]
        public string? Url { get; set; }

        [MaxLength(100)]
        public string? Tipo { get; set; }

        public DateTime? FechaRegistro { get; set; }

        [ForeignKey(nameof(Proyecto))]
        public int IdProyecto { get; set; }

        public Proyecto? Proyecto { get; set; }
    }
}
