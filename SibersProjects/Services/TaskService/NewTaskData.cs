using System.ComponentModel.DataAnnotations;

namespace SibersProjects.Services.TaskService;

public class NewTaskData
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    [Required] public int ProjectId { get; set; }
}