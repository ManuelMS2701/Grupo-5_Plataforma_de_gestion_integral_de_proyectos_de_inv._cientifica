using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ResearchHub.Models
{
    public class Variable
    {
        [Key]
        public int IdVariable { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Tipo { get; set; }

        [MaxLength(50)]
        public string? Unidad { get; set; }

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        [Precision(18, 2)]
        public decimal? RangoMin { get; set; }

        [Precision(18, 2)]
        public decimal? RangoMax { get; set; }

        public ICollection<Resultado> Resultados { get; set; } = new List<Resultado>();
    }
}
