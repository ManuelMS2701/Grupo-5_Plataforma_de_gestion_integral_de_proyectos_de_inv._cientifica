using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class SublineaInvestigacion
    {
        [Key]
        public int IdSublinea { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        public bool Activa { get; set; }

        [ForeignKey(nameof(LineaInvestigacion))]
        public int IdLinea { get; set; }

        public LineaInvestigacion? LineaInvestigacion { get; set; }

        public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
    }
}
