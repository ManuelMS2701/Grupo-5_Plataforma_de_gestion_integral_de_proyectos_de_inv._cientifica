namespace ResearchHub.Models
{
    public class BitacoraPageViewModel
    {
        public IReadOnlyList<BitacoraProyecto> Entradas { get; init; } = Array.Empty<BitacoraProyecto>();
        public IReadOnlyList<Proyecto> Proyectos { get; init; } = Array.Empty<Proyecto>();
        public int? ProyectoSeleccionado { get; init; }
    }
}
