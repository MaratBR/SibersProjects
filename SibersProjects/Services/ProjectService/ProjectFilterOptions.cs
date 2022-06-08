using Microsoft.AspNetCore.Mvc;

namespace SibersProjects.Services.ProjectService;

public class ProjectFilterOptions
{
    public enum SortByEnum
    {
        Name,
        StartsAt,
        EndsAt,
        Priority,
        ProjectManager,
        Contractor,
        Client,
        Default = Name
    }
    
    // NOTE: оба значения включительно
    
    public DateTime? StartsAfter { get; set; }
    public DateTime? EndsBefore { get; set; }
    public SortByEnum SortBy { get; set; } = SortByEnum.Default;
    public List<string>? ProjectManagers { get; set; }
    public string? Contractor { get; set; }
    public string? Client { get; set; }
}