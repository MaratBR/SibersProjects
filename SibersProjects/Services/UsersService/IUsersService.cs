using System.Security.Claims;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Utils;

namespace SibersProjects.Services.UsersService;

public interface IUsersService
{
    Task<User> Create(NewUserOptions options);
    Task<User> Update(User user, UpdateUserOptions options);
    Task<User> GetOrCreateDefaultUser();
    DefaultUserSettings GetDefaultUserSettings();
    Task<User?> GetUser(ClaimsPrincipal claimsPrincipal);
    Task<Pagination<UserDto>> PaginateUsers(UserPaginationOptions options);
    Task<UserDto?> GetById(string id);
}