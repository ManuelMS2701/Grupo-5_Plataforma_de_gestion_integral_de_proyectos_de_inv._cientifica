using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class Analisis
    {
        [Key]
        public int IdAnalisis { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Metodo { get; set; }

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        public DateTime? Fecha { get; set; }

        [MaxLength(2000)]
        public string? Conclusiones { get; set; }

        [ForeignKey(nameof(Resultado))]
        public int IdResultado { get; set; }

        public Resultado? Resultado { get; set; }

        public ICollection<Validacion> Validaciones { get; set; } = new List<Validacion>();
    }
}
