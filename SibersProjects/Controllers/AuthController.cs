using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.TokenService;
using SibersProjects.Services.UsersService;

namespace SibersProjects.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private const string InvalidCredentials = "Неверный логин или пароль";
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;
    private readonly UserManager<User> _userManager;
    private readonly IUsersService _usersService;


    public AuthController(UserManager<User> userManager, ITokenService tokenService, IUsersService usersService,
        IMapper mapper)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _usersService = usersService;
        _mapper = mapper;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Login);
        if (user == null)
        {
            if (request.Login.ToUpper() == _usersService.GetDefaultUserSettings().UserName.ToUpper())
                user = await _usersService.GetOrCreateDefaultUser();
            else
                return Unauthorized(InvalidCredentials);
        }

        if (await _userManager.CheckPasswordAsync(user, request.Password))
            return new LoginResponse
            {
                Token = await _tokenService.GenerateUserToken(user)
            };

        return Unauthorized(InvalidCredentials);
    }

    [Authorize]
    [HttpGet("whoami")]
    public async Task<IActionResult> WhoAmI()
    {
        // Примечание: возвращаем всю информацию о пользоателя чисто для дебага
        var subClaim = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
        User? user = null;
        if (subClaim != null)
            user = await _userManager.Users.Where(u => u.Id == subClaim).Include(u => u.Roles).FirstOrDefaultAsync();
        return Ok(new
        {
            Claims = User.Claims.Select(claim => new[] { claim.Type, claim.Value }),
            User = user != null ? _mapper.Map<User, UserDto>(user) : null
        });
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

    // TODO: refresh token
}