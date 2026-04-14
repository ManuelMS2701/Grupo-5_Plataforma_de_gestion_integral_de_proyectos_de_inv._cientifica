using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class TareaInvestigacion
    {
        [Key]
        public int IdTarea { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        [MaxLength(50)]
        public string? Estado { get; set; }

        [MaxLength(50)]
        public string? Prioridad { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaLimite { get; set; }

        public DateTime? FechaCierre { get; set; }

        [ForeignKey(nameof(Proyecto))]
        public int IdProyecto { get; set; }

        [ForeignKey(nameof(Experimento))]
        public int? IdExperimento { get; set; }

        [ForeignKey(nameof(Responsable))]
        public int? IdResponsable { get; set; }

        public Proyecto? Proyecto { get; set; }
        public Experimento? Experimento { get; set; }
        public Investigador? Responsable { get; set; }
    }
}
