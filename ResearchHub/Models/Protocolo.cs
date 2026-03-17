using System.ComponentModel.DataAnnotations;

namespace ResearchHub.Models
{
    public class Protocolo
    {
        [Key]
        public int IdProtocolo { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Version { get; set; }

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        public DateTime? FechaAprobacion { get; set; }

        public bool Activo { get; set; }

        public ICollection<Experimento> Experimentos { get; set; } = new List<Experimento>();
    }
}
