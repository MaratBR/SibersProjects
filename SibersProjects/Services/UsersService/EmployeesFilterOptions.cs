namespace SibersProjects.Services.UsersService;

public class EmployeesFilterOptions
{
    public enum SortByEnum
    {
        FirsName
    }

    public SortByEnum Type { get; set; }
}