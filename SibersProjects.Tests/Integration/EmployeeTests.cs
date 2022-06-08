using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Tests.Integration.Fixtures;

namespace SibersProjects.Tests.Integration;

public class EmployeeTests : AuthenticationTestsBase
{
    
    [InlineData("Patronymic", "UserWithPatronymic")]
    [InlineData(null, "UserWithoutPatronymic")]
    [Theory]
    public async Task TestNewEmployee(string? patronymic, string userName)
    {
        await CreateDefaultUserAndLoginClient();
        var response = await Client.PostAsync("/api/Employees", JsonContent.Create(new
        {
            firstName = "1",
            lastName = "1",
            password = "Qwerty01$",
            userName,
            email = "qwert@qwer.ru",
            patronymic = patronymic
        }));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var data = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(data);
        Assert.NotNull(data!.Id);
        Assert.Equal(userName, data.UserName);
        var userManager = Application.Services.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByIdAsync(data.Id);
        Assert.NotNull(user);
        var dbContext = Application.Services.GetRequiredService<AppDbContext>();
        await dbContext.SaveChangesAsync();
        Assert.Equal(2, await dbContext.Users.CountAsync());
        await userManager.DeleteAsync(user);
    }

    [Fact]
    public async Task TestEmployeesList()
    {
        
    }

    
}