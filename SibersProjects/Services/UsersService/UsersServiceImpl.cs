using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.RoleHelperService;
using SibersProjects.Services.UsersService.Exceptions;
using SibersProjects.Utils;

namespace SibersProjects.Services.UsersService;

public class UsersServiceImpl : IUsersService
{
    private readonly IMapper _mapper;
    private readonly IRoleHelperService _roleHelperService;
    private readonly UserManager<User> _userManager;

    public UsersServiceImpl(UserManager<User> userManager, IRoleHelperService roleHelperService, IMapper mapper)
    {
        _userManager = userManager;
        _roleHelperService = roleHelperService;
        _mapper = mapper;
    }

    public Task<User?> GetUser(ClaimsPrincipal claimsPrincipal)
    {
        return _userManager.FindByIdAsync(claimsPrincipal.GetUserId())!;
    }

    public async Task<Pagination<UserDto>> PaginateUsers(UserPaginationOptions options)
    {
        return new Pagination<UserDto>
        {
            Page = options.Page,
            PageSize = options.PageSize,
            Items = await GetUsersQueryable(options)
                .Skip(options.PageSize * (options.Page - 1))
                .Take(options.PageSize)
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                .ToListAsync()
        };
    }

    public Task<UserDto?> GetById(string id)
    {
        return _userManager.Users.Where(u => u.Id == id).ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
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

        if (!result.Succeeded) throw new IdentityUserException(result.Errors);

        foreach (var role in options.Roles)
        {
            await _roleHelperService.EnsureRoleExists(role);
        }

        await _userManager.AddToRolesAsync(user, options.Roles);
        
        return user;
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
            foreach (var role in newRoles) await _roleHelperService.EnsureRoleExists(role);

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
            Email = settings.Email
        };
        // хэшируем пароль в обход валидации
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, settings.Password);
        await _userManager.CreateAsync(user);
        await _roleHelperService.EnsureRoleExists(RoleNames.Superuser);
        await _userManager.AddToRoleAsync(user, RoleNames.Superuser);
        return user;
        ;
    }

    public DefaultUserSettings GetDefaultUserSettings()
    {
        return new DefaultUserSettings();
    }

    #region Utils

    private IQueryable<User> GetUsersQueryable(UserFilterOptions options)
    {
        var queryable = _userManager.Users;

        switch (options.SortBy)
        {
            case UserFilterOptions.SortByEnum.NewestToOldest:
                queryable = queryable.OrderByDescending(u => u.CreatedAt);
                break;
            case UserFilterOptions.SortByEnum.OldestToNewest:
                queryable = queryable.OrderBy(u => u.CreatedAt);
                break;
            case UserFilterOptions.SortByEnum.Name:
                queryable = queryable.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ThenBy(u => u.Patronymic);
                break;
        }

        if (options.Roles != null && options.Roles.Count > 0)
            queryable = queryable.Where(u => u.Roles.Any(r => options.Roles.Contains(r.Name)));

        return queryable;
    }

    #endregion
}