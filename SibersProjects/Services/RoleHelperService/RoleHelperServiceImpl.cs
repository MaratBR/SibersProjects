using Microsoft.AspNetCore.Identity;
using SibersProjects.Models;

namespace SibersProjects.Services.RoleHelperService;

public class RoleHelperServiceImpl : IRoleHelperService
{
    private readonly RoleManager<Role> _roleManager;
    
    public RoleHelperServiceImpl(RoleManager<Role> roleManager)
    {
        _roleManager = roleManager;
    }
    
    public async Task EnsureRoleExists(string name)
    {
        if (await _roleManager.RoleExistsAsync(name)) return;
        await _roleManager.CreateAsync(new Role
        {
            Name = name
        });
    }
}