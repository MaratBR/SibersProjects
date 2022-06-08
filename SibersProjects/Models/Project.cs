namespace SibersProjects.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string ClientCompany { get; set; } = string.Empty;

    // NOTE: на будущее лучше сделать ClientCompanyId как foreign key к таблице Companies (TODO?)
    public string ContractorCompany { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string? ProjectManagerId { get; set; } = string.Empty;
    public User? ProjectManager { get; set; } = null!;
    public ICollection<User> Employees { get; set; } = null!;
    public ICollection<WorkTask> Tasks { get; set; } = null!;
}