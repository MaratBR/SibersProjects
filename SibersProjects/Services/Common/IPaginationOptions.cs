namespace SibersProjects.Services.Common;

public interface IPaginationOptions
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}