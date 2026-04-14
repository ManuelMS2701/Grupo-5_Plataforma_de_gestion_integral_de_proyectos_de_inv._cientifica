using System.ComponentModel.DataAnnotations;

namespace ResearchHub.Models
{
    public class RolSistema
    {
        [Key]
        public int IdRol { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<UsuarioSistema> Usuarios { get; set; } = new List<UsuarioSistema>();
    }
}
