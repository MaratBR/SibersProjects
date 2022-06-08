using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SibersProjects.Models;
using SibersProjects.Services.UsersService;
using SibersProjects.Tests.Integration.Fixtures;

namespace SibersProjects.Tests.Integration;

public class AuthenticationTestsBase
{
    protected readonly PlaygroundApplication Application;
    protected readonly HttpClient Client;

    public AuthenticationTestsBase()
    {
        Application = new PlaygroundApplication();
        Client = Application.CreateClient();
    }

    
    protected async Task<(User, string)> CreateDefaultUser()
    {
        var usersService = Application.Server.Services.GetRequiredService<IUsersService>();
        var user = await usersService.GetOrCreateDefaultUser();
        return (user, usersService.GetDefaultUserSettings().Password);
    }
    
    protected async Task<User> CreateDefaultUserAndLoginClient()
    {
        var (user, password) = await CreateDefaultUser();
        var response = await Client.PostAsync("/api/Auth/login",
            JsonContent.Create(new { login = user.UserName, password }));
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(data);
        Assert.True(data!.ContainsKey("token"));

        var token = data["token"];
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return user;
    }
    
    protected async Task<User> CreateUser(string userName, string password)
    {
        var userManager = Application.Services.GetRequiredService<UserManager<User>>();
        var user = new User
        {
            UserName = userName,
            Email = userName + "@qwerty.ru",
        };
        user.PasswordHash = userManager.PasswordHasher.HashPassword(user, password);
        await userManager.CreateAsync(user);
        return user;
    }

}