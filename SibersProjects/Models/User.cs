using Microsoft.AspNetCore.Identity;

namespace SibersProjects.Models;

public class User : IdentityUser
{
    public const string DefaultUsername = "Admin";
    public const string DefaultPassword = "admin";
    
    public ICollection<Role> Roles { get; set; } = new List<Role>();

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    // Отчество не обязательно, но не будем его отмечать как nullable
    // потому что тогда в контроллере придется использовать пустую строку как 
    // показатель отсутствия значения
    public string? Patronymic { get; set; } = string.Empty;

    public ICollection<Project> Projects { get; set; } = null!;
}