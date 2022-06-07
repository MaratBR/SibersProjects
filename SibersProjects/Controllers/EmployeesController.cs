using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.UsersService;
using SibersProjects.Services.UsersService.Exceptions;
using SibersProjects.Utils;

namespace SibersProjects.Controllers;

[ApiController]
//[Authorize(Roles = RoleNames.Superuser)]
[Route("api/[controller]")]
public class EmployeesController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IUsersService _usersService;
    private readonly IMapper _mapper;
    
    public EmployeesController(UserManager<User> userManager, IUsersService usersService, IMapper mapper)
    {
        _userManager = userManager;
        _usersService = usersService;
        _mapper = mapper;
    }

    public class GetEmployeesOptions : UsersFilterOptions
    {
        [Range(1, 2000)]
        public int Page { get; set; } = 1;
        [Range(20, 100)] public int PageSize { get; set; } = 50;

    }
    
    [HttpGet]
    public async Task<PaginationResponse<UserDto>> GetEmployees([FromQuery] GetEmployeesOptions options)
    {
        var employees = await _usersService.GetUsersQueryable(options)
            .Skip(options.PageSize * (options.Page - 1))
            .Take(options.PageSize)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
        return new PaginationResponse<UserDto>
        {
            Items = employees,
            Page = options.Page,
            PageSize = options.PageSize
        };
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
            foreach (var error in e.Errors)  
            {
                model.AddModelError(error.Code, error.Description);
            }

            return BadRequest(model);
        }

    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userManager.Users.Where(u => u.Id == id)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
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