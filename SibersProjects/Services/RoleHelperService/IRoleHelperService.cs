namespace SibersProjects.Services.RoleHelperService;

public interface IRoleHelperService
{
    Task EnsureRoleExists(string name);
}