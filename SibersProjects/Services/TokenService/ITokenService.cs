using SibersProjects.Models;

namespace SibersProjects.Services.TokenService;

public interface ITokenService
{
    string GenerateUserToken(User user);
}