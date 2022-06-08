using System.Linq.Expressions;
using SibersProjects.Controllers;
using SibersProjects.Models;

namespace SibersProjects.Dto;

public class ProjectDetailsDto : ProjectBaseDto
{
    public List<UserDto> Employees { get; set; } = new();
    public UserDto? ProjectManager { get; set; } = null!;
    public List<TaskDto> Tasks { get; set; } = new();
    public int TasksTotal { get; set; }

}