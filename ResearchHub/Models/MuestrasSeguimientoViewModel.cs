namespace ResearchHub.Models
{
    public class MuestrasSeguimientoViewModel
    {
        public IReadOnlyList<Muestra> Muestras { get; init; } = Array.Empty<Muestra>();
        public IReadOnlyDictionary<int, IReadOnlyList<SeguimientoMuestra>> Seguimientos { get; init; } = new Dictionary<int, IReadOnlyList<SeguimientoMuestra>>();
    }
}
