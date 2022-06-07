namespace SibersProjects.Services.ProjectService;

public class ProjectUpdateData
{
    public string? Name { get; set; }
    public int? Priority { get; set; }
    public string? ProjectManagerId { get; set; }
    public string? ClientCompany { get; set; }
    public string? ContractorCompany { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}