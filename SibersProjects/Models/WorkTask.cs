namespace SibersProjects.Models;

public class WorkTask
{
    public enum StatusEnum : byte
    {
        ToDo,
        InProgress,
        Done
    }

    public int Id { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public User Author { get; set; } = null!;
    public string? AssigneeId { get; set; }
    public User? Assignee { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public StatusEnum Status { get; set; } = StatusEnum.ToDo;
}