using System.Security.Claims;
using SibersProjects.Models;

namespace SibersProjects.Services.PermissionsService;

public interface IPermissionsService
{
    Task<bool> CanUpdateStatus(ClaimsPrincipal userPrincipal, WorkTask task);
}