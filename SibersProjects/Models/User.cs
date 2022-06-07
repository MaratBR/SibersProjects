using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SibersProjects.Models;

public class User : IdentityUser
{
    public ICollection<Role> Roles { get; set; } = new List<Role>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    // Отчество не обязательно, но не будем его отмечать как nullable
    // потому что тогда в контроллере придется использовать пустую строку как 
    // показатель отсутствия значения
    public string? Patronymic { get; set; } = string.Empty;

    public ICollection<Project> Projects { get; set; } = null!;
}