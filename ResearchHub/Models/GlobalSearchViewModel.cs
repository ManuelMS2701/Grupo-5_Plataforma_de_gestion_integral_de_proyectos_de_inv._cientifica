namespace ResearchHub.Models
{
    public class SearchHitViewModel
    {
        public string Modulo { get; init; } = string.Empty;
        public string Titulo { get; init; } = string.Empty;
        public string? Subtitulo { get; init; }
        public string Controller { get; init; } = string.Empty;
        public string Action { get; init; } = "Details";
        public int Id { get; init; }
    }

    public class GlobalSearchViewModel
    {
        public string? Query { get; init; }
        public IReadOnlyList<SearchHitViewModel> Hits { get; init; } = Array.Empty<SearchHitViewModel>();
    }
}
