using System.Linq.Expressions;
using SibersProjects.Models;

namespace SibersProjects.Dto;

public class ProjectListItemDto : ProjectBaseDto
{
    public int EmployeesTotal { get; set; }
    public UserDto? ProjectManager { get; set; } = null!;
}