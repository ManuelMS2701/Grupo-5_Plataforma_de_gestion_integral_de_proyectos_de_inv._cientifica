using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class SeguimientoMuestra
    {
        [Key]
        public int IdSeguimiento { get; set; }

        public DateTime FechaRegistro { get; set; }

        [Required, MaxLength(100)]
        public string Estado { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Ubicacion { get; set; }

        [MaxLength(2000)]
        public string? Observaciones { get; set; }

        [ForeignKey(nameof(Muestra))]
        public int IdMuestra { get; set; }

        [ForeignKey(nameof(Usuario))]
        public int? IdUsuario { get; set; }

        public Muestra? Muestra { get; set; }
        public UsuarioSistema? Usuario { get; set; }
    }
}
