using SibersProjects.Models;

namespace SibersProjects.Services.TaskService;

public class TaskUpdateData
{
    public string? Description { get; set; }
    public WorkTask.StatusEnum? Status { get; set; }
    public string? Name { get; set; }
}