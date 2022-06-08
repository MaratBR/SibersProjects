namespace SibersProjects.Services.UsersService;

public class UpdateUserOptions
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string? Patronymic { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string>? Roles { get; set; }
}