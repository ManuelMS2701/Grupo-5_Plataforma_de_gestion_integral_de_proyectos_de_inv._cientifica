namespace ResearchHub.Models
{
    public class MiEspacioViewModel
    {
        public string NombreUsuario { get; init; } = string.Empty;
        public string Rol { get; init; } = string.Empty;
        public int TotalProyectos { get; init; }
        public int TotalExperimentos { get; init; }
        public int TotalResultados { get; init; }
        public int TotalPublicaciones { get; init; }
        public int TotalHitosPendientes { get; init; }
        public int TotalTareasPendientes { get; init; }
        public IReadOnlyList<Proyecto> ProyectosRecientes { get; init; } = Array.Empty<Proyecto>();
        public IReadOnlyList<Experimento> ExperimentosActivos { get; init; } = Array.Empty<Experimento>();
        public IReadOnlyList<Cronograma> HitosPendientes { get; init; } = Array.Empty<Cronograma>();
        public IReadOnlyList<Resultado> ResultadosRecientes { get; init; } = Array.Empty<Resultado>();
        public IReadOnlyList<Publicacion> PublicacionesRecientes { get; init; } = Array.Empty<Publicacion>();
        public IReadOnlyList<TareaInvestigacion> TareasPendientes { get; init; } = Array.Empty<TareaInvestigacion>();
    }
}
