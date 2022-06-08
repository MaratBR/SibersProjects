using System.Security.Claims;
using SibersProjects.Models;

namespace SibersProjects.Services.UsersService;

public interface IUsersService
{
    Task<User> Create(NewUserOptions options);
    Task<User> Update(User user, UpdateUserOptions options);
    Task<User> GetOrCreateDefaultUser();
    DefaultUserSettings GetDefaultUserSettings();
    IQueryable<User> GetUsersQueryable(UsersFilterOptions options);
    Task<User?> GetUser(ClaimsPrincipal claimsPrincipal);
}