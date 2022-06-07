using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SibersProjects.Models;
using SibersProjects.Services.UsersService;
using SibersProjects.Tests.Integration.Fixtures;

namespace SibersProjects.Tests.Integration;

public class AuthenticationTestsBase : IClassFixture<PlaygroundApplication>
{
    protected readonly PlaygroundApplication Application;
    protected readonly HttpClient Client;

    public AuthenticationTestsBase(PlaygroundApplication application)
    {
        Application = application;
        Client = application.CreateClient();
    }

    
    protected async Task<(User, string)> CreateDefaultUser()
    {
        var usersService = Application.Server.Services.GetRequiredService<IUsersService>();
        var user = await usersService.CreateDefaultUser();
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

}