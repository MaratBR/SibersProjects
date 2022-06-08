using SibersProjects.Models;


namespace SibersProjects.Services.TaskService;

public class TaskFilterOptions
{
    public enum SortByEnum
    {
        Priority,
        NewestToOldest,
        OldestToNewest
    }
    
    public WorkTask.StatusEnum? Status { get; set; }
    public SortByEnum SortBy { get; set; } = SortByEnum.NewestToOldest;
    public int? ProjectId { get; set; }

} 