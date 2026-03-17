using System.ComponentModel.DataAnnotations;

namespace ResearchHub.Models
{
    public class LineaInvestigacion
    {
        [Key]
        public int IdLinea { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        public bool Activa { get; set; }

        public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
    }
}
