using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class BitacoraProyecto
    {
        [Key]
        public int IdBitacora { get; set; }

        public DateTime FechaRegistro { get; set; }

        [Required, MaxLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required, MaxLength(4000)]
        public string Contenido { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Categoria { get; set; }

        [ForeignKey(nameof(Proyecto))]
        public int IdProyecto { get; set; }

        [ForeignKey(nameof(Usuario))]
        public int? IdUsuario { get; set; }

        public Proyecto? Proyecto { get; set; }
        public UsuarioSistema? Usuario { get; set; }
    }
}
