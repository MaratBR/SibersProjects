using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SibersProjects.Models;
using SibersProjects.Services;
using SibersProjects.Services.TokenService;
using SibersProjects.Services.UsersService;

namespace SibersProjects.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IUsersService _usersService;

    public AuthController(UserManager<User> userManager, ITokenService tokenService, IUsersService usersService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _usersService = usersService;
    }

    public class LoginRequest
    {
        [Required] public string Login { get; set; } = string.Empty;

        [Required] public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    private const string UserNotFoundMessage = "Пользователь не найден, проверьте правильность пароля и логина";

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Login);
        if (user == null)
        {
            if (request.Login == _usersService.GetDefaultUserSettings().UserName)
            {
                user = await _usersService.CreateDefaultUser();
            }
            else
            {
                return NotFound(UserNotFoundMessage);
            }
        }

        if (await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return new LoginResponse
            {
                Token = await _tokenService.GenerateUserToken(user)
            };
        }

        return NotFound(UserNotFoundMessage);
    }

    [Authorize]
    [HttpGet("whoami")]
    public async Task<IActionResult> WhoAmI()
    {
        var subClaim = User.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;
        User? user = null;
        if (subClaim != null)
            user = await _userManager.FindByIdAsync(subClaim);
        return Ok(new
        {
            Claims = User.Claims.Select(claim => new [] { claim.Type, claim.Value }),
            User = user
        });
    }
    
    // TODO: refresh token
}