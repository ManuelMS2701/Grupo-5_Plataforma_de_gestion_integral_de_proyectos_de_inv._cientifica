namespace ResearchHub.Models
{
    public class MisTareasViewModel
    {
        public IReadOnlyList<TareaInvestigacion> Tareas { get; init; } = Array.Empty<TareaInvestigacion>();
        public IReadOnlyList<Proyecto> Proyectos { get; init; } = Array.Empty<Proyecto>();
        public IReadOnlyList<Experimento> Experimentos { get; init; } = Array.Empty<Experimento>();
        public int? ProyectoSeleccionado { get; init; }
        public int? ExperimentoSeleccionado { get; init; }
    }
}
