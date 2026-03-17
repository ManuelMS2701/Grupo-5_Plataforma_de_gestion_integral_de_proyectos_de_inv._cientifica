using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class Validacion
    {
        [Key]
        public int IdValidacion { get; set; }

        public DateTime? Fecha { get; set; }

        [MaxLength(50)]
        public string? Resultado { get; set; }

        [MaxLength(2000)]
        public string? Observaciones { get; set; }

        [MaxLength(150)]
        public string? Validador { get; set; }

        [ForeignKey(nameof(Analisis))]
        public int IdAnalisis { get; set; }

        public Analisis? Analisis { get; set; }
    }
}
