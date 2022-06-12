using System.ComponentModel.DataAnnotations;

namespace SibersProjects.Services.ProjectService;

public class NewProjectOptions
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required] public int Priority { get; set; }
    [Required] public string ContractorCompany { get; set; } = string.Empty;
    [Required] public string ClientCompany { get; set; } = string.Empty;
    public string? ProjectManagerId { get; set; } = string.Empty;
    [Required] public DateTime StartsAt { get; set; }
    [Required] public DateTime EndsAt { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
}