using System.Linq.Expressions;
using SibersProjects.Models;

namespace SibersProjects.Dto;

public class ProjectBaseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContractorCompany { get; set; } = string.Empty;
    public string ClientCompany { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

}