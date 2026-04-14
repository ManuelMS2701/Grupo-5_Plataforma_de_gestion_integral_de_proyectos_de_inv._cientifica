using System.ComponentModel.DataAnnotations;

namespace ResearchHub.Models
{
    public class UsuarioSistemaFormViewModel
    {
        public int? IdUsuario { get; set; }

        [Required, MaxLength(150)]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required, MaxLength(200), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        [Display(Name = "Investigador vinculado")]
        public int? IdInvestigador { get; set; }

        public bool Activo { get; set; } = true;
    }
}
