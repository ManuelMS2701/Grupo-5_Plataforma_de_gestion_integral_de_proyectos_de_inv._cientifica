using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class Investigador
    {
        [Key]
        public int IdInvestigador { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(150)]
        public string? Especialidad { get; set; }

        [MaxLength(50)]
        public string? Orcid { get; set; }

        public DateTime FechaRegistro { get; set; }

        public bool Activo { get; set; }

        [ForeignKey(nameof(Institucion))]
        public int IdInstitucion { get; set; }

        public Institucion? Institucion { get; set; }

        public ICollection<Proyecto> ProyectosPrincipales { get; set; } = new List<Proyecto>();
    }
}
