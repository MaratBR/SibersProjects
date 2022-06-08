using System.Security.Claims;
using SibersProjects.Models;
using SibersProjects.Services.UsersService.Exceptions;

namespace SibersProjects.Services.UsersService;

public static class UsersServiceExtensions
{
    public static async Task<User> RequireUser(this IUsersService usersService, ClaimsPrincipal claimsPrincipal)
    {
        var user = await usersService.GetUser(claimsPrincipal);
        if (user == null)
            throw new CurrentUserMissingException();
        return user;
    }
}