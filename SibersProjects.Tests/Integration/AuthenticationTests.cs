using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SibersProjects.Models;
using SibersProjects.Services.UsersService;
using SibersProjects.Tests.Integration.Fixtures;

namespace SibersProjects.Tests.Integration;


public class AuthenticationTests : AuthenticationTestsBase
{
    [Fact]
    public async Task Authentication()
    {
        await CreateDefaultUserAndLoginClient();
        var response = await Client.GetAsync("/api/Auth/whoami");
        response.EnsureSuccessStatusCode();
    }

    public AuthenticationTests(PlaygroundApplication application) : base(application)
    {
    }
}