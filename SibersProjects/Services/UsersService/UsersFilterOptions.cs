namespace SibersProjects.Services.UsersService;

public class UsersFilterOptions
{
    public enum SortByEnum
    {
        Name,
        OldestToNewest,
        NewestToOldest
    }

    public SortByEnum SortBy { get; set; } = SortByEnum.NewestToOldest;
    public List<string>? Roles { get; set; }
}