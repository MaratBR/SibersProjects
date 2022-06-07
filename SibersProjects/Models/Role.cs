using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace SibersProjects.Models;

public class Role : IdentityRole
{
    [JsonIgnore] public ICollection<User> Users { get; set; } = null!;
}