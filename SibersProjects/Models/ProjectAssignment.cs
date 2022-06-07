namespace SibersProjects.Models;

public class ProjectAssignment
{
    public int ProjectId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;

    public Project Project { get; set; } = null!;
    public User Employee { get; set; } = null!;
}