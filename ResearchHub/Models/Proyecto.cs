using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchHub.Models
{
    public class Proyecto
    {
        [Key]
        public int IdProyecto { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        [MaxLength(2000)]
        public string? ObjetivoGeneral { get; set; }

        [MaxLength(50)]
        public string? Estado { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public DateTime FechaCreacion { get; set; }

        [ForeignKey(nameof(InvestigadorPrincipal))]
        public int IdInvestigadorPrincipal { get; set; }

        [ForeignKey(nameof(Institucion))]
        public int IdInstitucion { get; set; }

        [ForeignKey(nameof(LineaInvestigacion))]
        public int IdLinea { get; set; }

        [ForeignKey(nameof(SublineaInvestigacion))]
        public int? IdSublinea { get; set; }

        public Investigador? InvestigadorPrincipal { get; set; }
        public Institucion? Institucion { get; set; }
        public LineaInvestigacion? LineaInvestigacion { get; set; }
        public SublineaInvestigacion? SublineaInvestigacion { get; set; }

        public ICollection<Experimento> Experimentos { get; set; } = new List<Experimento>();
        public ICollection<Muestra> Muestras { get; set; } = new List<Muestra>();
        public ICollection<Publicacion> Publicaciones { get; set; } = new List<Publicacion>();
        public ICollection<RepositorioDatos> Repositorios { get; set; } = new List<RepositorioDatos>();
        public ICollection<Colaborador> Colaboradores { get; set; } = new List<Colaborador>();
        public ICollection<Cronograma> Cronogramas { get; set; } = new List<Cronograma>();
        public ICollection<TareaInvestigacion> Tareas { get; set; } = new List<TareaInvestigacion>();
        public ICollection<BitacoraProyecto> Bitacora { get; set; } = new List<BitacoraProyecto>();
    }
}
