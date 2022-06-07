namespace SibersProjects.Utils;

public class PaginationResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
}