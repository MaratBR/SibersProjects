using Microsoft.AspNetCore.Identity;
using SibersProjects.Models;
using SibersProjects.Services.UsersService.Exceptions;

namespace SibersProjects.Services.UsersService;

public class UsersServiceImpl : IUsersService
{
    private readonly UserManager<User> _userManager;
    
    public UsersServiceImpl(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    /// <param name="options"></param>
    /// <returns>Новый ползовател</returns>
    /// <exception cref="IdentityUserException">Если UserManager.CreateAsync вернет ошибки</exception>
    public async Task<User> Create(NewUserOptions options)
    {
        var user = new User
        {
            FirstName = options.FirstName,
            LastName = options.LastName,
            Patronymic = options.Patronymic,
            UserName = options.UserName,
            Email = options.Email
        };
        
        var result = await _userManager.CreateAsync(user, options.Password);

        if (result.Succeeded)
        {
            return user;
        }

        throw new IdentityUserException(result.Errors);
    }

    public async Task<User> Update(User user, UpdateUserOptions options)
    {
        if (options.Patronymic != string.Empty)
            user.Patronymic = options.Patronymic;
        if (options.FirstName != string.Empty)
            user.FirstName = options.FirstName;
        if (options.LastName != string.Empty)
            user.LastName = options.LastName;
        if (options.Email != string.Empty)
            user.Email = options.Email;
        await _userManager.UpdateAsync(user);
        return user;
    }

    public async Task<User> CreateDefaultUser()
    {
        var settings = GetDefaultUserSettings();
        var user = new User
        {
            FirstName = settings.UserName,
            LastName = settings.UserName,
            Patronymic = settings.UserName,
            UserName = settings.UserName,
            Email = settings.Email,
        };
        // хэшируем пароль в обход валидации
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, settings.Password);
        await _userManager.CreateAsync(user);
        return user;
;    }

    public DefaultUserSettings GetDefaultUserSettings()
    {
        return new DefaultUserSettings();
    }
}