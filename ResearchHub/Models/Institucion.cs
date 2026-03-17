using System.ComponentModel.DataAnnotations;

namespace ResearchHub.Models
{
    public class Institucion
    {
        [Key]
        public int IdInstitucion { get; set; }

        [Required, MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Pais { get; set; }

        [MaxLength(100)]
        public string? Ciudad { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        public ICollection<Investigador> Investigadores { get; set; } = new List<Investigador>();
        public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
    }
}
