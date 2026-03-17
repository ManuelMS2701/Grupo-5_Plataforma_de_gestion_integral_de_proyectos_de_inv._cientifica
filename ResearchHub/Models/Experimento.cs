using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class Experimento
    {
        [Key]
        public int IdExperimento { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [MaxLength(50)]
        public string? Estado { get; set; }

        [ForeignKey(nameof(Proyecto))]
        public int IdProyecto { get; set; }

        [ForeignKey(nameof(Protocolo))]
        public int IdProtocolo { get; set; }

        [ForeignKey(nameof(Laboratorio))]
        public int IdLaboratorio { get; set; }

        public Proyecto? Proyecto { get; set; }
        public Protocolo? Protocolo { get; set; }
        public Laboratorio? Laboratorio { get; set; }

        public ICollection<Resultado> Resultados { get; set; } = new List<Resultado>();
    }
}
