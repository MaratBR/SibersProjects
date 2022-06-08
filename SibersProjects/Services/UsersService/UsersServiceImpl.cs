using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SibersProjects.Models;
using SibersProjects.Services.RoleHelperService;
using SibersProjects.Services.UsersService.Exceptions;
using SibersProjects.Utils;

namespace SibersProjects.Services.UsersService;

public class UsersServiceImpl : IUsersService
{
    private readonly IRoleHelperService _roleHelperService;
    private readonly UserManager<User> _userManager;
    
    public UsersServiceImpl(UserManager<User> userManager, IRoleHelperService roleHelperService)
    {
        _userManager = userManager;
        _roleHelperService = roleHelperService;
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
        if (options.Roles != null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles.Except(options.Roles));
            var newRoles = options.Roles.Except(roles).ToList();
            foreach (var role in newRoles)
            {
                await _roleHelperService.EnsureRoleExists(role);
            }

            await _userManager.AddToRolesAsync(user, newRoles);
        }
        return user;
    }

    public async Task<User> GetOrCreateDefaultUser()
    {
        var settings = GetDefaultUserSettings();
        var user = await _userManager.FindByNameAsync(settings.UserName);
        if (user != null) return user;
        user = new User
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
        await _roleHelperService.EnsureRoleExists(RoleNames.Superuser);
        await _userManager.AddToRoleAsync(user, RoleNames.Superuser);
        return user;
;    }

    public DefaultUserSettings GetDefaultUserSettings()
    {
        return new DefaultUserSettings();
    }

    public IQueryable<User> GetUsersQueryable(UsersFilterOptions options)
    {
        IQueryable<User> queryable = _userManager.Users;

        switch (options.SortBy)
        {
            case UsersFilterOptions.SortByEnum.NewestToOldest:
                queryable = queryable.OrderByDescending(u => u.CreatedAt);
                break;
            case UsersFilterOptions.SortByEnum.OldestToNewest:
                queryable = queryable.OrderBy(u => u.CreatedAt);
                break;
            case UsersFilterOptions.SortByEnum.Name:
                queryable = queryable.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ThenBy(u => u.Patronymic);
                break;
        }

        if (options.Roles != null && options.Roles.Count > 0)
        {
            queryable = queryable.Where(u => u.Roles.Any(r => options.Roles.Contains(r.Name)));
        }

        return queryable;
    }

    public Task<User?> GetUser(ClaimsPrincipal claimsPrincipal)
    {
        return _userManager.FindByIdAsync(claimsPrincipal.GetUserId())!;
    }
}