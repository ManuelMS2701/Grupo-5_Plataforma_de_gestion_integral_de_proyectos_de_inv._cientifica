namespace ResearchHub.Models
{
    public class PagedListViewModel<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public string? Query { get; init; }
        public string? Sort { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public int TotalItems { get; init; }

        public int TotalPages =>
            PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));

        public int FromItem => TotalItems == 0 ? 0 : ((Page - 1) * PageSize) + 1;
        public int ToItem => Math.Min(Page * PageSize, TotalItems);
    }
}
