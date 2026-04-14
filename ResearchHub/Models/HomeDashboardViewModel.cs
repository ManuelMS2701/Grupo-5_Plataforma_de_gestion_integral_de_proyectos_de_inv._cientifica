namespace ResearchHub.Models
{
    public class HomeDashboardViewModel
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        public int TotalProyectos { get; set; }
        public int ProyectosActivos { get; set; }
        public int TotalExperimentos { get; set; }
        public int TotalResultados { get; set; }
        public int TotalTareasPendientes { get; set; }
        public int TotalHitosPendientes { get; set; }

        public IReadOnlyList<DashboardProyectoItem> ProyectosRecientes { get; set; } = Array.Empty<DashboardProyectoItem>();
        public IReadOnlyList<DashboardTareaItem> TareasPendientes { get; set; } = Array.Empty<DashboardTareaItem>();
        public IReadOnlyList<DashboardHitoItem> HitosProximos { get; set; } = Array.Empty<DashboardHitoItem>();
        public IReadOnlyList<DashboardActividadItem> ActividadReciente { get; set; } = Array.Empty<DashboardActividadItem>();
    }

    public class DashboardProyectoItem
    {
        public int IdProyecto { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Institucion { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaInicio { get; set; }
    }

    public class DashboardTareaItem
    {
        public int IdTarea { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Proyecto { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaLimite { get; set; }
    }

    public class DashboardHitoItem
    {
        public int IdCronograma { get; set; }
        public string NombreFase { get; set; } = string.Empty;
        public string? Proyecto { get; set; }
        public DateTime? FechaObjetivo { get; set; }
        public string? Estado { get; set; }
    }

    public class DashboardActividadItem
    {
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string? Proyecto { get; set; }
        public DateTime Fecha { get; set; }
        public string Controller { get; set; } = string.Empty;
        public int Id { get; set; }
    }
}
