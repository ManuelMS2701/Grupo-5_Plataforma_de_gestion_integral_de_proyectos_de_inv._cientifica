using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class Colaborador
    {
        [Key]
        public int IdColaborador { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Tipo { get; set; }

        [MaxLength(100)]
        public string? Rol { get; set; }

        [ForeignKey(nameof(Proyecto))]
        public int IdProyecto { get; set; }

        public Proyecto? Proyecto { get; set; }
    }
}
