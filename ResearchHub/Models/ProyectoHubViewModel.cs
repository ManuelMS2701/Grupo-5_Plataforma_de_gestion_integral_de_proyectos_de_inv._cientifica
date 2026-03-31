namespace ResearchHub.Models
{
    public class ProyectoHubViewModel
    {
        public Proyecto Proyecto { get; init; } = new Proyecto();
        public string NivelAcceso { get; init; } = "Read";
        public int TotalExperimentos { get; init; }
        public int TotalMuestras { get; init; }
        public int TotalResultados { get; init; }
        public int TotalAnalisis { get; init; }
        public int TotalValidaciones { get; init; }
        public int TotalPublicaciones { get; init; }
        public int TotalRepositorios { get; init; }
        public int TotalColaboradores { get; init; }
        public int TotalCronograma { get; init; }
        public int HitosPendientes { get; init; }
        public int HitosCompletados { get; init; }
        public IReadOnlyList<Cronograma> ProximosHitos { get; init; } = Array.Empty<Cronograma>();
        public IReadOnlyList<HubEventoItem> EventosRecientes { get; init; } = Array.Empty<HubEventoItem>();
    }

    public class HubEventoItem
    {
        public string Tipo { get; init; } = string.Empty;
        public string Titulo { get; init; } = string.Empty;
        public DateTime Fecha { get; init; }
        public string? Detalle { get; init; }
    }
}
