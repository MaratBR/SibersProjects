namespace SibersProjects.Dto;

/// <summary>
/// Класс, содержащий максимум информации о задаче для отображения
/// страницы задачи
/// </summary>
public class TaskDetailsDto : TaskDto
{
    public UserDto? Assignee { get; set; } = new();
    public UserDto Author { get; set; } = new();

    public ProjectBaseDto Project { get; set; } = new();
}