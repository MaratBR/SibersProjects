using System.ComponentModel.DataAnnotations;

namespace SibersProjects.Services.UsersService;

public class NewUserOptions
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    [Required] public string UserName { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}