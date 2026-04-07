namespace ResearchHub.Models
{
    public class ProyectoRelacionadosViewModel
    {
        public Proyecto Proyecto { get; init; } = new Proyecto();
        public string Titulo { get; init; } = string.Empty;
        public string Tipo { get; init; } = string.Empty;
        public IReadOnlyList<string> Columnas { get; init; } = Array.Empty<string>();
        public IReadOnlyList<ProyectoRelacionadoRowViewModel> Filas { get; init; } = Array.Empty<ProyectoRelacionadoRowViewModel>();
    }

    public class ProyectoRelacionadoRowViewModel
    {
        public IReadOnlyList<string> Valores { get; init; } = Array.Empty<string>();
        public string? DetalleController { get; init; }
        public int? DetalleId { get; init; }
    }
}
