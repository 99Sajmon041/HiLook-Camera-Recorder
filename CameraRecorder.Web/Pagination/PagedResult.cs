namespace CameraRecorder.Web.Pagination;

public sealed class PagedResult<T>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalItems { get; set; }
    public int PagesCount => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool CanGoNext => Page < PagesCount;
    public bool CanGoPrevious => Page > 1;
    public List<T> Items { get; set; } = [];
}