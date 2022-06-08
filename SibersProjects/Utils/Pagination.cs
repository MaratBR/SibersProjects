namespace SibersProjects.Utils;

public class Pagination<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
}