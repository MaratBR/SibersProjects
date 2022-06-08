using SibersProjects.Models;

namespace SibersProjects.Dto;

/// <summary>
/// Базовый DTO класс задачи с минимумом информации
/// </summary>
public class TaskDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public WorkTask.StatusEnum Status { get; set; }
    public string Description { get; set; } = string.Empty;
}