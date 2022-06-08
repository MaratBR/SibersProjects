namespace SibersProjects.Dto;

public class ProjectDetailsDto : ProjectBaseDto
{
    public List<UserDto> Employees { get; set; } = new();
    public UserDto? ProjectManager { get; set; }
}