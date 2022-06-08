namespace SibersProjects.Dto;

public class ProjectListItemDto : ProjectBaseDto
{
    public int EmployeesTotal { get; set; }
    public UserDto? ProjectManager { get; set; } = null!;
}