using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class Resultado
    {
        [Key]
        public int IdResultado { get; set; }

        public DateTime FechaRegistro { get; set; }

        [MaxLength(200)]
        public string? Valor { get; set; }

        [MaxLength(2000)]
        public string? Observaciones { get; set; }

        [ForeignKey(nameof(Experimento))]
        public int IdExperimento { get; set; }

        [ForeignKey(nameof(Variable))]
        public int IdVariable { get; set; }

        public Experimento? Experimento { get; set; }
        public Variable? Variable { get; set; }

        public ICollection<Analisis> Analisis { get; set; } = new List<Analisis>();
    }
}
