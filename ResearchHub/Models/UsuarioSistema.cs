using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class UsuarioSistema
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required, MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required, MaxLength(200), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string PasswordHash { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public DateTime? UltimoAcceso { get; set; }

        public int IntentosFallidos { get; set; }

        public DateTime? BloqueadoHasta { get; set; }

        [ForeignKey(nameof(Rol))]
        public int IdRol { get; set; }

        [ForeignKey(nameof(Investigador))]
        public int? IdInvestigador { get; set; }

        public RolSistema? Rol { get; set; }
        public Investigador? Investigador { get; set; }
    }
}
