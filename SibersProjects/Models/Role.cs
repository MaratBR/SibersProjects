using Microsoft.AspNetCore.Identity;

namespace SibersProjects.Models;

public class Role : IdentityRole
{
    public ICollection<User> Users { get; set; } = null!;
}