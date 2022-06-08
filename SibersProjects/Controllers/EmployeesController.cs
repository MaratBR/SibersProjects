using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.UsersService;
using SibersProjects.Services.UsersService.Exceptions;
using SibersProjects.Utils;

namespace SibersProjects.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Superuser)]
[Route("api/[controller]")]
public class EmployeesController : Controller
{
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly IUsersService _usersService;

    public EmployeesController(UserManager<User> userManager, IUsersService usersService, IMapper mapper)
    {
        _userManager = userManager;
        _usersService = usersService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<Pagination<UserDto>> GetEmployees([FromQuery] UserPaginationOptions options)
    {
        return await _usersService.PaginateUsers(options);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NewUserOptions options)
    {
        try
        {
            var user = await _usersService.Create(options);
            return Created(Url.Action(nameof(GetUser), new { id = user.Id })!, _mapper.Map<User, UserDto>(user));
        }
        catch (IdentityUserException e)
        {
            var model = new ModelStateDictionary();
            foreach (var error in e.Errors) model.AddModelError(error.Code, error.Description);

            return BadRequest(model);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _usersService.GetById(id);
        if (user == null) return NotFound($"Пользователь {id} не найден");
        return Ok(user);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserOptions options)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound($"Пользователь {id} не найден");

        await _usersService.Update(user, options);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound($"Пользователь {id} не найден");

        await _userManager.DeleteAsync(user);
        return Ok();
    }
}