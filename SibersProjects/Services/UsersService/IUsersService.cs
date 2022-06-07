using SibersProjects.Models;

namespace SibersProjects.Services.UsersService;

public interface IUsersService
{
    Task<User> Create(NewUserOptions options);
    Task<User> Update(User user, UpdateUserOptions options);
    Task<User> CreateDefaultUser();
    DefaultUserSettings GetDefaultUserSettings();
}