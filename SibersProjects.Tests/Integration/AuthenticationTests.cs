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
}