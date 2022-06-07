using SibersProjects.Models;

namespace SibersProjects.Services.TokenService;

public interface ITokenService
{
    Task<string> GenerateUserToken(User user);
}